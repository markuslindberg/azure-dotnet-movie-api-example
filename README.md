# Movies API

Movies API example using .NET 8 and [FastEndpoints](https://fast-endpoints.com/).

[![Main Workflow](https://github.com/markuslindberg/azure-dotnet-movie-api-example/actions/workflows/pipeline.yml/badge.svg)](https://github.com/markuslindberg/azure-dotnet-movie-api-example/actions/workflows/pipeline.yml)

[![CodeQL Scanning](https://img.shields.io/github/code-scanning/alerts/markuslindberg/azure-dotnet-movie-api-example?label=CodeQL%20Scanning)](https://github.com/markuslindberg/azure-dotnet-movie-api-example/security/code-scanning)

## Endpoints

List of REST endpoints:

| Paths | Method | Description|
| :---  | :---   | :---       |
|/movies|GET|Get all movies|
|/movies|POST|Create a movie|
|/movies/{movieId}|GET|Get a movie|
|/movies/{movieId}|PUT|Update a movie|
|/movies/{movieId}|DELETE|Delete a movie|
|/movies/{movieId}/characters|GET|Get movie characters|
|/movies/{movieId}/directors|GET|Get movie directors|

## Single Table Data Model

The domain objects (MOVIE, CHARACTER, DIRECTOR) in this example are stored in a single table:

| Partition Key | Row Key       | Attributes | | | | | |
| :---          | :---          | :---       | :--- | :--- | :--- | :--- | :--- |
| MOVIE_1 | MOVIE_1 | <b>Title</b><br>Little Big Adventure | <b>Category</b><br>Adventure | <b>Year</b><br>2024 | <b>Runtime</b><br>120 | <b>Rating</b><br>7.5 | <b>MovieId</b><br>1 |
| MOVIE_1 | CHARACTER_1 | <b>Name</b><br>Markus Lindberg | <b>Played By</b><br>Markus Lindberg | <b>Role</b><br>Protagonist | | | <b>CharacterId</b><br>1 |
| MOVIE_1 | CHARACTER_2 | <b>Name</b><br>Markus Lindberg | <b>Played By</b><br>Markus Lindberg | <b>Role</b><br>Antagonist | | | <b>CharacterId</b><br>2 |
| MOVIE_1 | DIRECTOR_1 | <b>Name</b><br>Markus Lindberg | | | | | <b>DirectorId</b><br>1 |
| CATEGORY_Adventure | 2024_1 | <b>Title</b><br>Little Big Adventure | <b>Category</b><br>Adventure | <b>Year</b><br>2024 | <b>Runtime</b><br>120 | <b>Rating</b><br>7.5 | <b>MovieId</b><br>1 |

| Access Patterns                   | Filter Expression |
| :---                              | :--- |
| Get movie                         | pk eq {movieId} and RowKey eq {movieId} |
| Get characters for a given movie  | pk eq {movieId} and RowKey gt 'CHARACTER#' and RowKey lt 'CHARACTER#~' |
| Get directors for a given movie   | pk eq {movieId} and RowKey gt 'DIRECTOR#' and RowKey lt 'DIRECTOR#~' |
| Get movies for a given category and year range | pk eq {category} and RowKey ge {yearMin} and RowKey le {yearMax} |

## Observability
OpenTelemetry-based data collection with Azure Monitor and Application Insights.

## Quality

### Testing
Integration testing using xUnit, FastEndpoints.Testing, and Verify for easy snapshot assertion.

* [xUnit](https://xunit.net/)
* [FastEndpoints.Testing](https://fast-endpoints.com/docs/integration-unit-testing#integration-testing)
* [Verify](https://github.com/VerifyTests/Verify)

### Static Code Analyzers
Using .NET source code analysis which is enabled by default, and the following third-party analyzers:

* [SonarAnalyzer.CSharp](https://github.com/SonarSource/sonar-dotnet)
* [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)

### Software Bill Of Materials (SBOM)
Generating SPDX 2.2 compatible SBOM using [Microsoft SBOM Tool](https://github.com/microsoft/sbom-tool) in GitHub Actions. The SBOM is saved as an artifact, and uploaded to GitHub's dependency submission API.