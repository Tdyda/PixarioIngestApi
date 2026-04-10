using System.Text.Json;
using Pixario.Ingest.Application.Ports.Integrations;
using Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Http;

namespace Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Clients;

public class ComfyUiCheckImageStatusGateway(
    ComfyUiPromptStatusSender sender) : IComfyUiCheckImageStatusGateway
{
    public async Task<JsonDocument?> ProcessAsync(string promptId, CancellationToken ct)
    {
        return await sender.SendAsync(promptId, ct);
    }
}