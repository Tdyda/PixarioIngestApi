using System.Text.Json.Serialization;

namespace Pixario.Ingest.Infrastructure.Integrations.Outbound.Http.ResponseDto;

public class StatusDto
{
    [JsonPropertyName("status_str")] public string? StatusStr { get; set; }

    [JsonPropertyName("completed")] public bool Completed { get; set; }

    [JsonPropertyName("messages")] public List<MessageDto>? Messages { get; set; }
}