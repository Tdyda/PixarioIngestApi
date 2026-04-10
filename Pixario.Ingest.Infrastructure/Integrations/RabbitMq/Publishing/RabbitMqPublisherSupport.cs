using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Connection;
using RabbitMQ.Client;

namespace Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Publishing;

public class RabbitMqPublisherSupport(
    IRabbitMqConnection conn,
    IOptionsMonitor<RabbitMqOptions> opt)
{
    public async Task PublishAsync<TMessage>(
        TMessage message,
        Func<IChannel, ReadOnlyMemory<byte>, BasicProperties, CancellationToken, Task> publishAction,
        CancellationToken ct)
    {
        await using var channel = await conn.Connection.CreateChannelAsync(cancellationToken: ct);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var props = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await publishAction(channel, body, props, ct);
    }
}