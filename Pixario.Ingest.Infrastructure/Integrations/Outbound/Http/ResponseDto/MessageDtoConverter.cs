using System.Text.Json;
using System.Text.Json.Serialization;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Http.ResponseDto;

namespace Pixario.Ingest.Application.Features.Worker.ImageProcessing;

public class MessageDtoConverter : JsonConverter<MessageDto>
{
    public override MessageDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();

        var eventName = reader.GetString();

        reader.Read();

        var data = JsonSerializer.Deserialize<MessageDataDto>(ref reader, options);

        reader.Read();

        return new MessageDto
        {
            Event = eventName,
            Data = data
        };
    }

    public override void Write(Utf8JsonWriter writer, MessageDto value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.Event);
        JsonSerializer.Serialize(writer, value.Data, options);
        writer.WriteEndArray();
    }
}