using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Exceptions;
using Pixario.Ingest.Application.Features.Worker.ImageProcessing;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pixario.Ingest.Worker.Messaging;

public class BatchProcessedConsumer(
    IRabbitMqConnection conn,
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<RabbitMqOptions> opt,
    ILogger<BatchProcessedConsumer> log) : BackgroundService
{
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _channel = await conn.Connection.CreateChannelAsync(cancellationToken: ct);

        await _channel.BasicQosAsync(0, 1, false, ct);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg = JsonSerializer.Deserialize<BatchProcessedMessage>(json);

                if (msg is null) throw new PermanentProcessingException("Invalid completed message payload");

                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<BatchProcessedHandler>();

                var response = await handler.Handle(msg, ct);
                var body = await response.Content.ReadAsStringAsync(ct);

                log.LogDebug(
                    "Status: {StatusCode}, Content-Type: {ContentType}, body: {Body}",
                    response.StatusCode,
                    response.Content.Headers.ContentType?.ToString(),
                    body
                );

                await _channel.BasicAckAsync(ea.DeliveryTag, false, ct);
            }
            catch (PermanentProcessingException ex)
            {
                log.LogError(ex, "Permanent failure -> DLQ");
                await PublishDlqAsync(ea.Body, ct);
                await _channel.BasicAckAsync(ea.DeliveryTag, false, ct);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Transient failure -> retry");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, ct);
            }
        };

        await _channel.BasicConsumeAsync(
            opt.CurrentValue.BatchProcessedNotifyQueue,
            false,
            consumer,
            ct);
    }

    private async Task PublishDlqAsync(ReadOnlyMemory<byte> body, CancellationToken ct)
    {
        await using var ch = await conn.Connection.CreateChannelAsync(cancellationToken: ct);

        var props = new BasicProperties
        {
            Persistent = true,
            Headers = new Dictionary<string, object?>
            {
                ["dlq_reason"] = "permanent_failure"
            }
        };

        await ch.BasicPublishAsync(
            opt.CurrentValue.Exchange,
            opt.CurrentValue.BatchProcessedNotifyDlqRoutingKey,
            false,
            props,
            body,
            ct);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}