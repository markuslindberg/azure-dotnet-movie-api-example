using FastEndpoints;
using MoviesApi.Contracts.Data;

namespace MoviesApi.Contracts.Requests;

public record UpdateMovieRequest(Ulid MovieId, [property: FromBody] MovieDto Movie);