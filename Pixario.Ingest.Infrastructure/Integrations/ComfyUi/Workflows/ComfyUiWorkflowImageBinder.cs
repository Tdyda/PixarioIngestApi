using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Configuration;

namespace Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Workflows;

public class ComfyUiWorkflowImageBinder(IOptionsMonitor<ComfyUiConfig> comfyOpt)
{
    public void SetImageInWorkflow(JsonNode workflow, string fileName)
    {
        var nodeId = comfyOpt.CurrentValue.LoadImageNodeId;

        var imageNode = workflow[nodeId]
                        ?? throw new InvalidOperationException($"Workflow node '{nodeId}' does not exist.");

        var inputsNode = imageNode["inputs"]
                         ?? throw new InvalidOperationException($"Workflow node '{nodeId}' does not contain 'inputs'.");

        inputsNode["image"] = fileName;
    }
}