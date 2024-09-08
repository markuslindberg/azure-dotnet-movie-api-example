namespace MoviesApi.Domain;

public record Movie(Ulid MovieId, string Title, string Category, int Year, int Runtime, double Rating)
{
  public List<Character> Characters { get; init; } = [];
  public List<Director> Directors { get; init; } = [];
}
