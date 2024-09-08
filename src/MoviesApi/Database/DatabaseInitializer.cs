using Azure.Data.Tables;
using MoviesApi.Domain;
using MoviesApi.Repositories;

namespace MoviesApi.Database;

public class DatabaseInitializer(TableClient tableClient, MovieRepository movieRepository)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await tableClient.CreateIfNotExistsAsync(ct);
        await movieRepository.UpsertMovieAsync(
            new Movie(Ulid.Parse("01J3HQ9334QXPPCX8T7WWSGTGN"), "The Taste of Things", "Drama", 2023, 135, 7.4)
            {
                Characters = [
                    new Character(Ulid.Parse("01J3HQAT8E30XM6CAAP2TCQG6Y"), "Dodin Bouffant", "Benoît Magimel", "Protagonist"),
                    new Character(Ulid.Parse("01J3HQC0QHNZVFYBVG5SAJKBG6"), "Eugénie", "Juliette Binoche", "Protagonist"),
                ],
                Directors = [
                    new Director(Ulid.Parse("01J3HQC9290A146275FV5T9Q7E"), "Tran Anh Hung"),
                ]
            }, ct);
        await movieRepository.UpsertMovieAsync(
            new Movie(Ulid.Parse("01J3HQA5BKKVMRVQFYECMNRAA9"), "Gladiator", "Adventure", 2000, 170, 8.5)
            {
                Characters = [
                    new Character(Ulid.Parse("01J3HQCHJT38Q0Q4VSHGYC3Q58"), "Maximus Meridius", "Russell Crowe", "Protagonist"),
                    new Character(Ulid.Parse("01J3HQCV284BDGEKXYS85SG08P"), "Emperor Commodus", "Joaquin Phoenix", "Antagonist"),
                ],
                Directors = [
                    new Director(Ulid.Parse("01J3HQD39VMC6WT0WERMD6SMN1"), "Ridley Scott"),
                ]
            }, ct);
    }
}