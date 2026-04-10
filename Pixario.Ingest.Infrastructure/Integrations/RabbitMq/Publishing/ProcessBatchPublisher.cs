using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports.Messaging;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Connection;
using RabbitMQ.Client;

namespace Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Publishing;

public sealed class ProcessBatchPublisher(IRabbitMqConnection conn, IOptions<RabbitMqOptions> options)
    : IProcessBatchPublisher
{
    private readonly RabbitMqOptions _opt = options.Value;

    public async Task PublishAsync(ProcessBatchMessage message, CancellationToken ct)
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

        await channel.BasicPublishAsync(
            _opt.Exchange,
            _opt.BatchProcessRoutingKey,
            false,
            props,
            body,
            ct);
    }
}