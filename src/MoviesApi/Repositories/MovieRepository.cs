using System.Runtime.CompilerServices;
using Azure.Data.Tables;
using MoviesApi.Domain;

namespace MoviesApi.Repositories;

public class MovieRepository(TableClient tableClient)
{
  public async Task<bool> CreateMovieAsync(Movie movie, CancellationToken ct = default)
  {
    var movieKey = $"MOVIE_{movie.MovieId}";
    var movieEntity = MovieToTableEntity(movie, movieKey, movieKey);
    var response = await tableClient.AddEntityAsync(movieEntity, ct);
    if (response.IsError)
    {
      return false;
    }

    movieEntity.PartitionKey = $"CATEGORY_{movie.Category}";
    movieEntity.RowKey = $"{movie.Year}_{movie.MovieId}";
    response = await tableClient.AddEntityAsync(movieEntity, ct);
    if (response.IsError)
    {
      return false;
    }

    foreach (var character in movie.Characters)
    {
      var characterEntity = CharacterToTableEntity(character, movieKey, $"CHARACTER_{character.CharacterId}");
      response = await tableClient.AddEntityAsync(characterEntity, ct);
      if (response.IsError)
      {
        return false;
      }
    }

    foreach (var director in movie.Directors)
    {
      var directorEntity = DirectorToTableEntity(director, movieKey, $"DIRECTOR_{director.DirectorId}");
      response = await tableClient.AddEntityAsync(directorEntity, ct);
      if (response.IsError)
      {
        return false;
      }
    }

    return true;
  }

  public async Task<bool> UpsertMovieAsync(Movie movie, CancellationToken ct = default)
  {
    var movieKey = $"MOVIE_{movie.MovieId}";

    var oldEntities = await tableClient.QueryAsync<TableEntity>(
      filter: $"PartitionKey eq '{movieKey}'",
      select: ["PartitionKey", "RowKey", "Category", "Year"],
      cancellationToken: ct).ToListAsync(ct);

    var rowKeysToUpdate = new[]
    {
      movie.Characters.Select(x => $"CHARACTER_{x.CharacterId}"),
      movie.Directors.Select(x => $"DIRECTOR_{x.DirectorId}"),
      [movieKey]
    }.SelectMany(x => x).ToHashSet();

    var movieActions = oldEntities
      .Where(x => !rowKeysToUpdate.Contains(x.RowKey))
      .Select(x => new TableTransactionAction(TableTransactionActionType.Delete, x))
      .ToList();

    var movieEntity = MovieToTableEntity(movie, movieKey, movieKey);
    movieActions.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, movieEntity));

    movieActions.AddRange(movie.Characters
      .Select(x => CharacterToTableEntity(x, movieKey, $"CHARACTER_{x.CharacterId}"))
      .Select(x => new TableTransactionAction(TableTransactionActionType.UpsertReplace, x)));

    movieActions.AddRange(movie.Directors
      .Select(x => DirectorToTableEntity(x, movieKey, $"DIRECTOR_{x.DirectorId}"))
      .Select(x => new TableTransactionAction(TableTransactionActionType.UpsertReplace, x)));

    if (!await SubmitTransactionAsync(movieActions, ct))
    {
      return false;
    }

    movieEntity.PartitionKey = $"CATEGORY_{movie.Category}";
    movieEntity.RowKey = $"{movie.Year}_{movie.MovieId}";
    var response = await tableClient.UpsertEntityAsync(movieEntity, TableUpdateMode.Replace, ct);
    if (response.IsError)
    {
      return false;
    }

    (string? oldCategory, int? oldYear) = oldEntities
      .Where(x => x.RowKey == movieKey)
      .Select(x => (x.GetString("Category"), x.GetInt32("Year")))
      .SingleOrDefault();

    if (oldCategory != movie.Category || oldYear != movie.Year)
    {
      response = await tableClient.DeleteEntityAsync($"CATEGORY_{oldCategory}", $"{oldYear}_{movie.MovieId}", cancellationToken: ct);
      if (response.IsError)
      {
        return false;
      }
    }

    return true;
  }

  public async Task<bool> DeleteMovieAsync(Ulid movieId, CancellationToken ct = default)
  {
    var movieKey = $"MOVIE_{movieId}";
    List<TableTransactionAction> deleteMovieActions = [];
    string? category = null;
    int? year = null;

    var entities = tableClient.QueryAsync<TableEntity>(
      filter: $"PartitionKey eq 'MOVIE_{movieId}'",
      select: ["PartitionKey", "RowKey", "Category", "Year"],
      cancellationToken: ct);

    await foreach (var entity in entities)
    {
      if (entity.RowKey == movieKey)
      {
        category = entity.GetString("Category");
        year = entity.GetInt32("Year");
      }

      deleteMovieActions.Add(new TableTransactionAction(
        TableTransactionActionType.Delete,
        new TableEntity(movieKey, entity.RowKey)));
    }

    var response = await tableClient.DeleteEntityAsync($"CATEGORY_{category}", $"{year}_{movieId}", cancellationToken: ct);
    if (response.IsError)
    {
      return false;
    }

    return await SubmitTransactionAsync(deleteMovieActions, ct);
  }

  public async Task<Movie?> GetMovieAsync(Ulid movieId, CancellationToken ct = default)
  {
    List<Character> characters = [];
    List<Director> directors = [];

    var result = tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq 'MOVIE_{movieId}'", cancellationToken: ct);

    await foreach (var entity in result)
    {
      if (entity.RowKey.StartsWith("CHARACTER_"))
      {
        characters.Add(new Character(
          Ulid.Parse(entity.RowKey[10..]),
          entity.GetString("Name"),
          entity.GetString("PlayedBy"),
          entity.GetString("Role")
        ));
      }
      else if (entity.RowKey.StartsWith("DIRECTOR_"))
      {
        directors.Add(new Director(
          Ulid.Parse(entity.RowKey[9..]),
          entity.GetString("Name")
        ));
      }
      else if (entity.RowKey.StartsWith("MOVIE_"))
      {
        return new Movie(
          Ulid.Parse(entity.RowKey[6..]),
          entity.GetString("Title"),
          entity.GetString("Category"),
          entity.GetInt32("Year") ?? 0,
          entity.GetInt32("Runtime") ?? 0,
          entity.GetDouble("Rating") ?? 0)
        {
          Characters = characters,
          Directors = directors,
        };
      }
    }

    return null;
  }

  public async IAsyncEnumerable<Movie> GetMoviesAsync(
    string category, int? yearMin, int? yearMax, [EnumeratorCancellation] CancellationToken ct = default)
  {
    string filter = $"PartitionKey eq 'CATEGORY_{category}'";

    if (yearMin != null)
    {
      filter += $" and RowKey ge '{yearMin}'";
    }

    if (yearMax != null)
    {
      filter += $" and RowKey lt '{yearMax + 1}'";
    }

    var result = tableClient.QueryAsync<TableEntity>(filter: filter, cancellationToken: ct);

    await foreach (var entity in result)
    {
      ct.ThrowIfCancellationRequested();

      yield return new Movie(
        Ulid.Parse(entity.RowKey[5..]),
        entity.GetString("Title"),
        entity.GetString("Category"),
        entity.GetInt32("Year") ?? 0,
        entity.GetInt32("Runtime") ?? 0,
        entity.GetDouble("Rating") ?? 0);
    }
  }

  private async Task<bool> SubmitTransactionAsync(IEnumerable<TableTransactionAction> actions, CancellationToken ct = default)
  {
    foreach (var chunk in actions.Chunk(100))
    {
      var response = (await tableClient.SubmitTransactionAsync(chunk, ct)).GetRawResponse();
      if (response.IsError)
      {
        return false;
      }
    }

    return true;
  }

  private static TableEntity MovieToTableEntity(Movie movie, string partitionKey, string rowKey)
  {
    return new TableEntity(partitionKey, rowKey)
    {
        { "MovieId", movie.MovieId.ToString() },
        { "Title", movie.Title },
        { "Category", movie.Category },
        { "Year", movie.Year },
        { "Runtime", movie.Runtime },
        { "Rating", movie.Rating },
    };
  }

  private static TableEntity CharacterToTableEntity(Character character, string partitionKey, string rowKey)
  {
    return new TableEntity(partitionKey, rowKey)
    {
        { "CharacterId", character.CharacterId.ToString() },
        { "Name", character.Name },
        { "PlayedBy", character.PlayedBy },
        { "Role", character.Role },
    };
  }

  private static TableEntity DirectorToTableEntity(Director director, string partitionKey, string rowKey)
  {
    return new TableEntity(partitionKey, rowKey)
    {
        { "DirectorId", director.DirectorId.ToString() },
        { "Name", director.Name },
    };
  }
}