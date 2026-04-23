using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Exceptions;
using Pixario.Ingest.Application.Features.Worker.ImageProcessing;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Core.Domain;
using Pixario.Ingest.Infrastructure.Exceptions;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pixario.Ingest.Worker.Messaging;

public class BatchProcessedDlqConsumer(
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
            RetouchBatch? batch = null;
            using var scope = scopeFactory.CreateScope();
            var batchRepository = scope.ServiceProvider.GetRequiredService<IBatchRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler = scope.ServiceProvider.GetRequiredService<BatchProcessedDlqHandler>();

            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg = JsonSerializer.Deserialize<BatchProcessedMessage>(json);

                if (msg is null)
                    throw new PermanentProcessingException("Invalid completed message payload");

                batch = await batchRepository.GetAsync(msg.BatchId, ct);

                if (batch is null)
                    throw new PermanentProcessingException(
                        $"Batch {msg.BatchId} not found in DLQ consumer");

                var response = await handler.Handle(msg, ct);
                var body = await response.Content.ReadAsStringAsync(ct);

                log.LogDebug(
                    "Status: {StatusCode}, body: {Body}",
                    response.StatusCode,
                    PrettyJson(body));
            }
            catch (ExternalServiceUnavailableException ex)
            {
                log.LogError(ex, "Transient failure (dlq) -> retry");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, ct);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error while handling DLQ message");
            }
            finally
            {
                if (batch is not null)
                {
                    batch.MarkFailed();
                    await batchRepository.UpdateAsync(batch, ct);
                    await unitOfWork.SaveChangesAsync(ct);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false, ct);
            }
        };

        await _channel.BasicConsumeAsync(
            opt.CurrentValue.BatchProcessedNotifyDlqQueue,
            false,
            consumer,
            ct);
    }

    private static string PrettyJson(string body)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<object>(body);
            return JsonSerializer.Serialize(parsed, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch
        {
            return body;
        }
    }
}