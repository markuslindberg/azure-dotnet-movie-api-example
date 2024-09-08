using FastEndpoints;
using MoviesApi.Contracts.Requests;

namespace MoviesApi.Validation;

public sealed class GetMoviesRequestValidator : Validator<GetMoviesRequest>
{
    public GetMoviesRequestValidator()
    {
        RuleFor(x => x.Category).NotEmpty();
    }
}