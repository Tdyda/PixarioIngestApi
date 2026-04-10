namespace Pixario.Ingest.Application.Ports.Integrations;

public interface IImageProcessingGateway
{
    Task<string> ProcessAsync(string fileName, CancellationToken ct);
}