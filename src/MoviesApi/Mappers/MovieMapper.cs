using MoviesApi.Contracts.Data;
using MoviesApi.Domain;
using Riok.Mapperly.Abstractions;

namespace MoviesApi.Mappers;

[Mapper]
public partial class MovieMapper
{
    public partial MovieDto MovieToMovieDto(Movie movie);
    public partial Movie MovieDtoToMovie(MovieDto movieDto);
}