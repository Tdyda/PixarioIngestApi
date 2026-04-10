namespace Pixario.Ingest.Infrastructure.Integrations.Outbound;

public sealed class CallbackOptions
{
    public string BaseUrl { get; init; } = string.Empty;
    public string EndpointPath { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
}