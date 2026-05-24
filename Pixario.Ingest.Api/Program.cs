using Pixario.Ingest.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices(builder.Configuration);

builder.AddPixarioLogging();

var app = builder.Build();

await app.ApplyMigrationsAsync();

await app.UsePipeline();

app.Run();