using Microsoft.Extensions.Options;
using Pixario.Ingest.Api.Features.Jobs;
using Pixario.Ingest.Api.OpenApi;
using Pixario.Ingest.Api.Security;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Connection;
using Scalar.AspNetCore;

namespace Pixario.Ingest.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task<WebApplication> UsePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
        {
            app.MapOpenApi();

            app.MapScalarApiReference(o =>
            {
                o.DisableAgent();

                o.WithOpenApiRoutePattern("/openapi/{documentName}.json")
                    .AddDocument(OpenApiDocs.ApiKey, "Main API (ApiKey)", isDefault: true)
                    .AddDocument(OpenApiDocs.Public, "Public API");
            });
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<ApiKeyMiddleware>();
        app.UseAuthorization();

        app.MapControllers();
        app.MapUploadFeatureApiKey();

        using (var scope = app.Services.CreateScope())
        {
            var options = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<RabbitMqOptions>>();
            var rabbit = scope.ServiceProvider.GetRequiredService<IRabbitMqConnection>();

            await using var channel = await rabbit.Connection.CreateChannelAsync();
            await RabbitMqTopology.EnsureCreatedAsync(channel, options.CurrentValue);
        }

        return app;
    }
}