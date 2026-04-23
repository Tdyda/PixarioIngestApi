using System.Text.Json.Serialization;

namespace Pixario.Ingest.Infrastructure.Integrations.Outbound.Http.ResponseDto;

[JsonConverter(typeof(MessageDtoConverter))]
public class MessageDto
{
    public string? Event { get; init; }
    public required MessageDataDto Data { get; init; }
}