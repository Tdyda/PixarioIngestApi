using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports;
using RabbitMQ.Client;

namespace Pixario.Ingest.Infrastructure.Messaging;

public sealed class RabbitMqPublisher : IQueuePublisher
{
    private readonly IRabbitMqConnection _conn;
    private readonly RabbitMqOptions _opt;

    public RabbitMqPublisher(IRabbitMqConnection conn, IOptions<RabbitMqOptions> options)
    {
        _conn = conn;
        _opt = options.Value;
    }

    public async Task PublishAsync(ImageProcessingMessage message, CancellationToken ct)
    {
        await using var channel = await _conn.Connection.CreateChannelAsync(cancellationToken: ct);

        await RabbitMqTopology.EnsureCreatedAsync(channel, _opt, ct);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var props = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await channel.BasicPublishAsync(
            exchange: _opt.Exchange,
            routingKey: _opt.RoutingKey,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct);
    }
}