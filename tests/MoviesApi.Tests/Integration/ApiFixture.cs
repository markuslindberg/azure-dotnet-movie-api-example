using FastEndpoints.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MoviesApi.Tests.Integration;

[DisableWafCache]
public class ApiFixture : AppFixture<Program>
{
    protected override ValueTask SetupAsync()
    {
        return ValueTask.CompletedTask;
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
        a.UseEnvironment("Development");
    }

    protected override void ConfigureServices(IServiceCollection s)
    {
    }
}