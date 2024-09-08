using MoviesApi.Contracts.Data;
using MoviesApi.Domain;
using Riok.Mapperly.Abstractions;

namespace MoviesApi.Mappers;

[Mapper]
public partial class CharacterMapper
{
    public partial CharacterDto CharacterToCharacterDto(Character character);
}