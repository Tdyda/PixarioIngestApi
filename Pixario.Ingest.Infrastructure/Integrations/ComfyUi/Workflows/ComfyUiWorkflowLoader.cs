using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Configuration;

namespace Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Workflows;

public class ComfyUiWorkflowLoader(
    IOptionsMonitor<ComfyUiConfig> comfyOpt
)
{
    public async Task<JsonNode> Load(CancellationToken ct)
    {
        var workflowBytes = await File.ReadAllBytesAsync(
            comfyOpt.CurrentValue.WorkflowPath,
            ct);

        return JsonNode.Parse(workflowBytes)
               ?? throw new InvalidOperationException("Nie udało się sparsować workflow.");
    }
}