using System.Net;
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

public sealed class CheckImageStatusConsumer(
    IServiceScopeFactory scopeFactory,
    IRabbitMqConnection conn,
    IOptionsMonitor<RabbitMqOptions> opt,
    ILogger<CheckImageStatusConsumer> log)
    : BackgroundService
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
                var msg = JsonSerializer.Deserialize<CheckImageStatusMessage>(json);

                if (msg is null)
                    throw new PermanentProcessingException("Invalid completed message payload");

                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<CheckImageStatusHandler>();
                var isSuccess = await handler.Handle(msg, ct);

                if (!isSuccess)
                {
                    log.LogDebug("Job {jobID} is still processing. Scheduling next status check in 10 seconds.", msg.JobId);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false, ct);
                    return;
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false, ct);
            }
            catch (PermanentProcessingException ex)
            {
                log.LogError(ex, "Permanent failure (notifier) -> DLQ");

                await PublishDlqAsync(ea.Body, ct);
                await _channel.BasicAckAsync(ea.DeliveryTag, false, ct);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Transient failure (notifier) -> retry");

                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, ct);
            }
        };

        await _channel.BasicConsumeAsync(
            opt.CurrentValue.ImageStatusCheckQueue,
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
                ["dlq_reason"] = "notifier_permanent_failure"
            }
        };

        await ch.BasicPublishAsync(
            opt.CurrentValue.Exchange,
            opt.CurrentValue.ImageStatusCheckDlqRoutingKey,
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