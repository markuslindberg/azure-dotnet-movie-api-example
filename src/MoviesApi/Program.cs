using System.Text.Json.Serialization;
using Azure.Data.Tables;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using FastEndpoints;
using FastEndpoints.Swagger;
using MoviesApi.Database;
using MoviesApi.Repositories;
using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

if (builder.Environment.IsProduction())
{
  builder.Services
    .AddOpenTelemetry()
    .UseAzureMonitor()
    .ConfigureResource(x => x.AddService(serviceName: "MoviesAPI"));
}

builder.Services
  .AddFastEndpoints()
  .SwaggerDocument(o =>
  {
    o.DocumentSettings = s =>
    {
      s.Title = "Movies API";
      s.Version = "v1";
      s.SchemaSettings.TypeMappers.Add(
        new PrimitiveTypeMapper(
          typeof(Ulid),
          schema =>
          {
            schema.Type = JsonObjectType.String;
            schema.Format = "ulid";
            schema.MinLength = 26;
            schema.MaxLength = 26;
          }));
    };
  });

builder.Services.AddTransient<MovieRepository>();
builder.Services.AddSingleton(x => new TableServiceClient(config["StorageConnectionString"]));
builder.Services.AddSingleton(x => x.GetRequiredService<TableServiceClient>().GetTableClient("Movies"));
builder.Services.AddSingleton<DatabaseInitializer>();

var app = builder.Build();

app.UseFastEndpoints(c =>
{
  c.Serializer.Options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
  c.Serializer.Options.Converters.Add(new Cysharp.Serialization.Json.UlidJsonConverter());
});
app.UseSwaggerGen();

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();

public partial class Program { }