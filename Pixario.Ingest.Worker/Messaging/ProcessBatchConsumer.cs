using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Exceptions;
using Pixario.Ingest.Application.Features.Worker.ImageProcessing;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pixario.Ingest.Worker.Messaging;

public sealed class ProcessBatchConsumer(
    IRabbitMqConnection conn,
    IOptionsMonitor<RabbitMqOptions> opt,
    ILogger<ProcessBatchConsumer> log,
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await conn.Connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg = JsonSerializer.Deserialize<ProcessBatchMessage>(json);

                if (msg is null)
                    throw new PermanentProcessingException("Invalid message payload");

                using var scope = scopeFactory.CreateScope();
                var batchRepository = scope.ServiceProvider.GetRequiredService<IBatchRepository>();

                var currentBatch = await batchRepository.GetProcessing();
                if (currentBatch is not null && currentBatch.Id != msg.BatchId)
                {
                    log.LogWarning($"Batch {currentBatch.Id} still in progress, redirect {msg.BatchId} to retry...");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);

                    return;
                }

                var handler = scope.ServiceProvider.GetRequiredService<ProcessBatchHandler>();

                await handler.Handle(msg, stoppingToken);

                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (PermanentProcessingException ex)
            {
                log.LogError(ex, "Permanent failure -> DLQ");
                await PublishDlqAsync(ea.Body, stoppingToken);
                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Transient failure -> retry");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            opt.CurrentValue.BatchProcessQueue,
            false,
            consumer,
            stoppingToken);
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
            opt.CurrentValue.BatchProcessDlqRoutingKey,
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