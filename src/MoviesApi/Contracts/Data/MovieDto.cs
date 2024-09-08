namespace MoviesApi.Contracts.Data;

public record MovieDto(Ulid MovieId, string Title, string Category, int Year, int Runtime, double Rating)
{
  public List<CharacterDto>? Characters { get; init; } = [];
  public List<DirectorDto>? Directors { get; init; } = [];
}