using FastEndpoints;
using MoviesApi.Contracts.Data;
using MoviesApi.Contracts.Requests;
using MoviesApi.Mappers;
using MoviesApi.Repositories;

namespace MoviesApi.Endpoints;

public class GetMovieCharactersEndpoint(MovieRepository repository) : Endpoint<GetMovieCharactersRequest, List<CharacterDto>>
{
    public override void Configure()
    {
        Get("movies/{MovieId}/characters");
        AllowAnonymous();
        Summary(s => s.Summary = "Get movie characters");
    }

    public override async Task HandleAsync(GetMovieCharactersRequest req, CancellationToken ct)
    {
        var movie = await repository.GetMovieAsync(req.MovieId, ct);
        if (movie == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var mapper = new CharacterMapper();
        await SendOkAsync(movie.Characters.Select(mapper.CharacterToCharacterDto).ToList(), ct);
    }
}