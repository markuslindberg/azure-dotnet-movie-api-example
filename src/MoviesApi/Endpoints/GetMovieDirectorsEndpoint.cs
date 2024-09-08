using FastEndpoints;
using MoviesApi.Contracts.Data;
using MoviesApi.Contracts.Requests;
using MoviesApi.Mappers;
using MoviesApi.Repositories;

namespace MoviesApi.Endpoints;

public class GetMovieDirectorsEndpoint(MovieRepository repository) : Endpoint<GetMovieDirectorsRequest, List<DirectorDto>>
{
    public override void Configure()
    {
        Get("movies/{MovieId}/directors");
        AllowAnonymous();
        Summary(s => s.Summary = "Get movie directors");
    }

    public override async Task HandleAsync(GetMovieDirectorsRequest req, CancellationToken ct)
    {
        var movie = await repository.GetMovieAsync(req.MovieId, ct);
        if (movie == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var mapper = new DirectorMapper();
        await SendOkAsync(movie.Directors.Select(mapper.DirectorToDirectorDto).ToList(), ct);
    }
}