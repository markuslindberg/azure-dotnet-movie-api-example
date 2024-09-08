using FastEndpoints;
using MoviesApi.Contracts.Requests;

namespace MoviesApi.Validation;

public sealed class UpdateMovieRequestValidator : Validator<UpdateMovieRequest>
{
    public UpdateMovieRequestValidator()
    {
        RuleFor(x => x.Movie).NotNull();
        RuleFor(x => x.Movie).SetValidator(new MovieDtoValidator());
        RuleFor(x => x.Movie.MovieId).Equal(x => x.MovieId);
    }
}