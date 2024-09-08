using FastEndpoints;
using MoviesApi.Contracts.Data;
using MoviesApi.Contracts.Requests;
using MoviesApi.Repositories;

namespace MoviesApi.Endpoints;

public class DeleteMovieEndpoint(MovieRepository repository) : Endpoint<DeleteMovieRequest, MovieDto>
{
    public override void Configure()
    {
        Delete("movies/{MovieId}");
        AllowAnonymous();
        Description(b => b
            .Produces(204)
            .ClearDefaultProduces(200));
        Summary(s => s.Summary = "Delete movie");
    }

    public override async Task HandleAsync(DeleteMovieRequest req, CancellationToken ct)
    {
        await repository.DeleteMovieAsync(req.MovieId, ct);
        await SendNoContentAsync(ct);
    }
}