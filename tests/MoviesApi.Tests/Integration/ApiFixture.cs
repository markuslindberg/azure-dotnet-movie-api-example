using FastEndpoints.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MoviesApi.Tests.Integration;

[DisableWafCache]
public class ApiFixture : AppFixture<Program>
{
    protected override Task SetupAsync()
    {
        DerivePathInfo((sourceFile, projectDirectory, type, method) => new (
            directory: Path.Combine(projectDirectory, "Snapshots"),
            typeName: type.Name,
            methodName: method.Name));
        return Task.CompletedTask;
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
        a.UseEnvironment("Development");
    }

    protected override void ConfigureServices(IServiceCollection s)
    {
    }
}