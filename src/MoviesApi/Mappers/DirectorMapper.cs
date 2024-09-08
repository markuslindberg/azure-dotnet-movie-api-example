using MoviesApi.Contracts.Data;
using MoviesApi.Domain;
using Riok.Mapperly.Abstractions;

namespace MoviesApi.Mappers;

[Mapper]
public partial class DirectorMapper
{
    public partial DirectorDto DirectorToDirectorDto(Director director);
}