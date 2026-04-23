namespace Pixario.Ingest.Infrastructure.Integrations.Outbound.Configuration;

public sealed class PixarioOptions
{
    public string BaseUrl { get; init; } = string.Empty;
    public string FilesProcessedSuccessfully { get; init; } = string.Empty;
    public string FilesProcessingFailed { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
}