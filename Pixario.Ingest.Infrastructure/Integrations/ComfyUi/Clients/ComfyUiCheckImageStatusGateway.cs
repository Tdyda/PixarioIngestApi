using System.Text.Json;
using Pixario.Ingest.Application.Extensions;
using Pixario.Ingest.Application.Features.Worker.ImageProcessing;
using Pixario.Ingest.Application.Ports.Integrations;
using Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Http;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Builders;

namespace Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Clients;

public class ComfyUiCheckImageStatusGateway(
    ComfyUiPromptStatusSender sender,
    CheckImageResponseBuilder builder) : IComfyUiCheckImageStatusGateway
{
    public async Task<CheckImageStatusDto?> ProcessAsync(string promptId, CancellationToken ct)
    {
        var document = await sender.SendAsync(promptId, ct);
        if (document == null) return null;

        return builder.Build(document, promptId);
    }
}