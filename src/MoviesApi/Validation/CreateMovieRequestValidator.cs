using FastEndpoints;
using MoviesApi.Contracts.Requests;

namespace MoviesApi.Validation;

public sealed class CreateMovieRequestValidator : Validator<CreateMovieRequest>
{
    public CreateMovieRequestValidator()
    {
        RuleFor(x => x.Movie).NotNull();
        RuleFor(x => x.Movie).SetValidator(new MovieDtoValidator());
    }
}