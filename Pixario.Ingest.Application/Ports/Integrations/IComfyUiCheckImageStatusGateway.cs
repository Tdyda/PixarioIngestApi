using Pixario.Ingest.Application.Features.Worker.ImageProcessing;

namespace Pixario.Ingest.Application.Ports.Integrations;

public interface IComfyUiCheckImageStatusGateway
{
    Task<CheckImageStatusDto?> ProcessAsync(string promptId, CancellationToken ct);
}