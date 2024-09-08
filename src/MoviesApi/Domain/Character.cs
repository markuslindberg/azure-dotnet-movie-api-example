namespace MoviesApi.Domain;

public record Character(Ulid CharacterId, string Name, string PlayedBy, string Role);
