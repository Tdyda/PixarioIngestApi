using System.Text.Json;

namespace Pixario.Ingest.Application.Ports.Integrations;

public interface IComfyUiCheckImageStatusGateway
{
    Task<JsonDocument?> ProcessAsync(string promptId, CancellationToken ct);
}