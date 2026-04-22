using System.Text.Json.Serialization;

namespace Pixario.Ingest.Infrastructure.Integrations.Outbound.Http.ResponseDto;

public class MessageDataDto
{
    [JsonPropertyName("prompt_id")] public string? PromptId { get; set; }

    [JsonPropertyName("timestamp")] public long? Timestamp { get; set; }

    [JsonPropertyName("nodes")] public List<string>? Nodes { get; set; }

    [JsonPropertyName("node_id")] public string? NodeId { get; set; }

    [JsonPropertyName("node_type")] public string? NodeType { get; set; }

    [JsonPropertyName("executed")] public List<string>? Executed { get; set; }

    [JsonPropertyName("exception_message")]
    public string? ExceptionMessage { get; set; }

    [JsonPropertyName("exception_type")] public string? ExceptionType { get; set; }

    [JsonPropertyName("traceback")] public List<string>? Traceback { get; set; }

    [JsonPropertyName("current_outputs")] public List<string>? CurrentOutputs { get; set; }
}