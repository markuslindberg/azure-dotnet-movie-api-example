namespace MoviesApi.Contracts.Requests;

public record GetMoviesRequest(string Category, int? YearMin = null, int? YearMax = null);