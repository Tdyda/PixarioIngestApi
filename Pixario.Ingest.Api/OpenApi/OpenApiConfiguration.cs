using Microsoft.OpenApi;

namespace Pixario.Ingest.Api.OpenApi;

public static class OpenApiConfiguration
{
    public static IServiceCollection AddCustomOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(OpenApiDocs.ApiKey, o =>
        {
            o.ShouldInclude = d => d.GroupName == OpenApiDocs.ApiKey;
            o.AddDocumentTransformer((doc, _, _) =>
            {
                doc.Info = new OpenApiInfo { Title = "Main API (ApiKey)", Version = "v1" };
                AddSecuritySchemes(doc);
                SetDocSecurity(doc, OpenApiDocs.ApiKeySchemeId);
                return Task.CompletedTask;
            });
        });

        services.AddOpenApi(OpenApiDocs.Public, o =>
        {
            o.ShouldInclude = d => d.GroupName == OpenApiDocs.Public;
            o.AddDocumentTransformer((doc, _, _) =>
            {
                doc.Info = new OpenApiInfo { Title = "Public API", Version = "v1" };

                return Task.CompletedTask;
            });
        });

        return services;
    }

    private static void AddSecuritySchemes(OpenApiDocument doc)
    {
        doc.Components ??= new OpenApiComponents();
        doc.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        doc.Components.SecuritySchemes[OpenApiDocs.ApiKeySchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = "x-api-key"
        };
    }

    private static void SetDocSecurity(OpenApiDocument doc, string schemeId)
    {
        doc.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(schemeId, doc)] = []
            }
        ];

        doc.SetReferenceHostDocument();
    }
}