using Pixario.Ingest.Application.Ports.Integrations;
using Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Http;
using Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Workflows;

namespace Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Clients;

public class ComfyUiProcessBatchGateway(
    ComfyUiWorkflowLoader loader,
    ComfyUiPromptPayloadBuilder payloadBuilder,
    ComfyUiWorkflowImageBinder binder,
    ComfyUiPromptSender sender
) : IImageProcessingGateway
{
    public async Task<string> ProcessAsync(string fileName, CancellationToken ct)
    {
        var workflow = await loader.Load(ct);

        binder.SetImageInWorkflow(workflow, fileName);

        var payload = payloadBuilder.Build(workflow);

        return await sender.SendAsync(payload, ct);
    }
}