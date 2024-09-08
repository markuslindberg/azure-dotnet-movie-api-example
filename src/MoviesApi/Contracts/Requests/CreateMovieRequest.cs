using FastEndpoints;
using MoviesApi.Contracts.Data;

namespace MoviesApi.Contracts.Requests;

public record CreateMovieRequest([property: FromBody] MovieDto Movie);