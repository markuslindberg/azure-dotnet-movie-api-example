using FastEndpoints;
using MoviesApi.Contracts.Data;
using MoviesApi.Contracts.Requests;
using MoviesApi.Mappers;
using MoviesApi.Repositories;

namespace MoviesApi.Endpoints;

public class UpdateMovieEndpoint(MovieRepository repository) : Endpoint<UpdateMovieRequest, MovieDto>
{
    public override void Configure()
    {
        Put("movies/{MovieId}");
        AllowAnonymous();
        Summary(s => s.Summary = "Update movie");
    }

    public override async Task HandleAsync(UpdateMovieRequest req, CancellationToken ct)
    {
        var mapper = new MovieMapper();
        var movie = mapper.MovieDtoToMovie(req.Movie);
        await repository.UpsertMovieAsync(movie, ct);
        await SendOkAsync(req.Movie, ct);
    }
}