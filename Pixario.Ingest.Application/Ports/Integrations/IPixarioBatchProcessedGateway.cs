using Pixario.Ingest.Core.Domain;

namespace Pixario.Ingest.Application.Ports.Integrations;

public interface IPixarioBatchProcessedGateway
{
    Task<HttpResponseMessage> ProcessAsync(RetouchBatch batch, CancellationToken ct);
}