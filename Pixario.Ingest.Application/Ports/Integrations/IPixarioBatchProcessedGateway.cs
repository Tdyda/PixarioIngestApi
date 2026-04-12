using Pixario.Ingest.Core.Entities;

namespace Pixario.Ingest.Application.Ports.Integrations;

public interface IPixarioBatchProcessedGateway
{
    Task<HttpResponseMessage> ProcessAsync(ImageRetouchBatch batch, CancellationToken ct);
}