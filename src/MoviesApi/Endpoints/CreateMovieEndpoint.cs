using FastEndpoints;
using MoviesApi.Contracts.Data;
using MoviesApi.Contracts.Requests;
using MoviesApi.Mappers;
using MoviesApi.Repositories;

namespace MoviesApi.Endpoints;

public class CreateMovieEndpoint(MovieRepository repository) : Endpoint<CreateMovieRequest, MovieDto>
{
    public override void Configure()
    {
        Post("movies");
        AllowAnonymous();
        Description(b => b
            .Produces<MovieDto>(201)
            .ClearDefaultProduces(200));
        Summary(s =>
        {
            s.Summary = "Create movie";
            s.ResponseHeaders.Add(new ResponseHeader(201, "Location"));
        });
    }

    public override async Task HandleAsync(CreateMovieRequest req, CancellationToken ct)
    {
        var mapper = new MovieMapper();
        var movie = mapper.MovieDtoToMovie(req.Movie);
        await repository.CreateMovieAsync(movie, ct);
        await SendCreatedAtAsync<GetMovieEndpoint>(
            new { movieId = req.Movie.MovieId }, req.Movie, null, null, generateAbsoluteUrl: true, ct);
    }
}