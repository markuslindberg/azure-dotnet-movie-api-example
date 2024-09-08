using FastEndpoints;
using MoviesApi.Contracts.Data;
using MoviesApi.Contracts.Requests;
using MoviesApi.Mappers;
using MoviesApi.Repositories;

namespace MoviesApi.Endpoints;

public class GetMoviesEndpoint(MovieRepository repository) : Endpoint<GetMoviesRequest, List<MovieDto>>
{
    public override void Configure()
    {
        Get("movies");
        AllowAnonymous();
        Summary(s => s.Summary = "Get movies");
    }

    public override async Task HandleAsync(GetMoviesRequest req, CancellationToken ct)
    {
        var mapper = new MovieMapper();
        var movies = await repository.GetMoviesAsync(req.Category, req.YearMin, req.YearMax, ct)
            .Select(mapper.MovieToMovieDto)
            .ToListAsync(ct);

        await SendOkAsync(movies, ct);
    }
}