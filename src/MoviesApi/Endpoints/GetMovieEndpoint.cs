using FastEndpoints;
using MoviesApi.Contracts.Data;
using MoviesApi.Contracts.Requests;
using MoviesApi.Mappers;
using MoviesApi.Repositories;

namespace MoviesApi.Endpoints;

public class GetMovieEndpoint(MovieRepository repository) : Endpoint<GetMovieRequest, MovieDto>
{
    public override void Configure()
    {
        Get("movies/{MovieId}");
        AllowAnonymous();
        Summary(s => s.Summary = "Get movie");
    }

    public override async Task HandleAsync(GetMovieRequest req, CancellationToken ct)
    {
        var movie = await repository.GetMovieAsync(req.MovieId, ct);
        if (movie == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var mapper = new MovieMapper();
        await SendOkAsync(mapper.MovieToMovieDto(movie), ct);
    }
}