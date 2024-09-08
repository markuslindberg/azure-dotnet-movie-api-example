namespace MoviesApi.Contracts.Data;

public record CharacterDto(Ulid CharacterId, string Name, string PlayedBy, string Role);