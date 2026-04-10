namespace Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Configuration;

public class ComfyUiConfig
{
    public string Url { get; set; } = "";
    public string WorkflowPath { get; set; } = "";
    public string LoadImageNodeId { get; set; } = "";
    public string SaveImageNodeId { get; set; } = "";
}