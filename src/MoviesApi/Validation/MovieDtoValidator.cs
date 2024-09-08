using FastEndpoints;
using MoviesApi.Contracts.Data;

namespace MoviesApi.Validation;

public sealed class MovieDtoValidator : Validator<MovieDto>
{
    public MovieDtoValidator()
    {
        RuleFor(x => x.MovieId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Category).NotEmpty();
        RuleFor(x => x.Year).GreaterThan(0);
        RuleFor(x => x.Runtime).GreaterThan(0);
        RuleFor(x => x.Rating).InclusiveBetween(0, 10);
    }
}