using System.Text.Json;
using System.Text.Json.Serialization;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Http.ResponseDto;

namespace Pixario.Ingest.Application.Features.Worker.ImageProcessing;

public class StatusDto
{
    [JsonPropertyName("status_str")]
    public string? StatusStr { get; set; }

    [JsonPropertyName("completed")]
    public bool Completed { get; set; }

    [JsonPropertyName("messages")]
    public List<MessageDto>? Messages { get; set; }
}