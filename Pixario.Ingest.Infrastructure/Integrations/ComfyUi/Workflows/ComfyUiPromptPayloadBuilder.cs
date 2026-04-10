using System.Text.Json.Nodes;

namespace Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Workflows;

public class ComfyUiPromptPayloadBuilder
{
    public JsonObject Build(JsonNode workflow)
    {
        return new JsonObject
        {
            ["prompt"] = workflow,
            ["client_id"] = Guid.NewGuid().ToString()
        };
    }
}