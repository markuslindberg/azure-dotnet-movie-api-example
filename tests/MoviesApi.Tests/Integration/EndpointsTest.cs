using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using MoviesApi.Contracts.Data;
using MoviesApi.Contracts.Requests;
using MoviesApi.Endpoints;

namespace MoviesApi.Tests.Integration;

public class EndpointsTest(ApiFixture api) : TestBase<ApiFixture>
{
    private static readonly Ulid TestMovieId = Ulid.Parse("01J6KMHE380F9EN141A4DR2F2N");
    private const string TestCategory = "Integration Test Category";
    private static MovieDto CreateTestMovie(Ulid id) =>
        new (id, "Test Movie", TestCategory, 2999, 120, 9.0)
        {
            Characters = [new CharacterDto(Ulid.Parse("01J6MZYQA6EMMKSTHVPEW7MC3K"), "Test Character", "Me", "Protagonist")],
            Directors = [new DirectorDto(Ulid.Parse("01J6N01XT3W3M2SNQCG9S0XGHE"), "Test Director")]
        };

    protected override async Task SetupAsync()
    {
        await base.SetupAsync();
        var request = new UpdateMovieRequest(TestMovieId, CreateTestMovie(TestMovieId));
        await api.Client.PUTAsync<UpdateMovieEndpoint, UpdateMovieRequest>(request);
    }

    protected override async Task TearDownAsync()
    {
        var request = new DeleteMovieRequest(TestMovieId);
        await api.Client.DELETEAsync<DeleteMovieEndpoint, DeleteMovieRequest>(request);
        await base.TearDownAsync();
    }

    [Fact]
    public async Task CreateMovieShouldMatchResponse()
    {
        var movieId = Ulid.Parse("01J6N09BPBVZNXT7P5ZF0VVE9M");
        var request = new CreateMovieRequest(CreateTestMovie(movieId));
        var (response, result) = await api.Client.POSTAsync<CreateMovieEndpoint, CreateMovieRequest, MovieDto>(request);

        await api.Client.DELETEAsync<DeleteMovieEndpoint, DeleteMovieRequest>(new DeleteMovieRequest(movieId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith($"/movies/{movieId}", response.Headers.GetValues("Location").SingleOrDefault());
        await Verify(result);
    }

    [Fact]
    public async Task DeleteMovieShouldMatchResponse()
    {
        var movieId = Ulid.Parse("01J6N09ZS5FCXKHMDV92BJ2AK9");
        await api.Client.POSTAsync<CreateMovieEndpoint, CreateMovieRequest, MovieDto>(
            new CreateMovieRequest(CreateTestMovie(movieId)));

        var request = new DeleteMovieRequest(movieId);
        var response = await api.Client.DELETEAsync<DeleteMovieEndpoint, DeleteMovieRequest>(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetMovieCharactersShouldMatchResponse()
    {
        var request = new GetMovieCharactersRequest(TestMovieId);
        var (response, result) = await api.Client.GETAsync<GetMovieCharactersEndpoint, GetMovieCharactersRequest, List<CharacterDto>>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await Verify(result);
    }

    [Fact]
    public async Task GetMovieDirectorsShouldMatchResponse()
    {
        var request = new GetMovieDirectorsRequest(TestMovieId);
        var (response, result) = await api.Client.GETAsync<GetMovieDirectorsEndpoint, GetMovieDirectorsRequest, List<DirectorDto>>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await Verify(result);
    }

    [Fact]
    public async Task GetMovieShouldMatchResponse()
    {
        var request = new GetMovieRequest(TestMovieId);
        var (response, result) = await api.Client.GETAsync<GetMovieEndpoint, GetMovieRequest, MovieDto>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await Verify(result);
    }

    [Fact]
    public async Task GetMoviesShouldMatchResponse()
    {
        var request = new GetMoviesRequest(TestCategory);
        var (response, result) = await api.Client.GETAsync<GetMoviesEndpoint, GetMoviesRequest, List<MovieDto>>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await Verify(result.Take(1));
    }

    [Fact]
    public async Task UpdateMovieShouldMatchResponse()
    {
        var request = new UpdateMovieRequest(TestMovieId, CreateTestMovie(TestMovieId));
        var (response, result) = await api.Client.PUTAsync<UpdateMovieEndpoint, UpdateMovieRequest, MovieDto>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await Verify(result);
    }

    [Theory]
    [InlineData(400, "{\"movieId\": \"ABC_$£!\"}")]
    [InlineData(400, "{\"invalid_json\"")]
    public async Task CreateMovieShouldMatchStatusCode(int expectedStatusCode, string body)
    {
        var requestBody = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var response = await api.Client.PostAsync("movies", requestBody);

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(400, "ABC_$£!")]
    public async Task DeleteMovieShouldMatchStatusCode(int expectedStatusCode, string movieId)
    {
        var response = await api.Client.DeleteAsync($"movies/{movieId}");

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(400, "ABC_$£!")]
    [InlineData(404, "01J6MQFGEC2NVYP4Z3R3V9MPMV")]
    public async Task GetMovieShouldMatchStatusCode(int expectedStatusCode, string movieId)
    {
        var response = await api.Client.GetAsync($"movies/{movieId}");

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(400, "")]
    [InlineData(400, null)]
    public async Task GetMoviesShouldMatchStatusCode(int expectedStatusCode, string? category)
    {
        var response = await api.Client.GetAsync($"movies?category={category}");

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(400, "ABC_$£!")]
    [InlineData(404, "01J6MQFGEC2NVYP4Z3R3V9MPMV")]
    public async Task GetMovieCharactersShouldMatchStatusCode(int expectedStatusCode, string movieId)
    {
        var response = await api.Client.GetAsync($"movies/{movieId}/characters");

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(400, "ABC_$£!")]
    [InlineData(404, "01J6MQFGEC2NVYP4Z3R3V9MPMV")]
    public async Task GetMovieDirectorsShouldMatchStatusCode(int expectedStatusCode, string movieId)
    {
        var response = await api.Client.GetAsync($"movies/{movieId}/directors");

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(400, "01J6MQ8PG4FN8GMJ4SY94MMRAS", "{\"movieId\": \"01J6MQ90RVZKYQVKF77R1RARVX\"}")]
    [InlineData(400, "01J6MQ8PG4FN8GMJ4SY94MMRAS", "{\"invalid_json\"")]
    [InlineData(400, "ABC_$£!", "{\"movieId\": \"ABC_$£!\"}")]
    public async Task UpdateMovieStatusCodeTheory(int expectedStatusCode, string movieId, string body)
    {
        var requestBody = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var response = await api.Client.PutAsync($"movies/{movieId}", requestBody);

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }
}