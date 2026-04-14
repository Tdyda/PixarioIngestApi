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
    ILogger<BatchProcessedConsumer> logger) : BackgroundService
{
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _channel = await conn.Connection.CreateChannelAsync(cancellationToken: ct);

        await _channel.BasicQosAsync(0, 1, false, ct);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        try
        {
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg = JsonSerializer.Deserialize<BatchProcessedMessage>(json);

                if (msg is null) throw new PermanentProcessingException("Invalid completed message payload");

                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<BatchProcessedHandler>();

                var response = await handler.Handle(msg, ct);
                var body = await response.Content.ReadAsStringAsync(ct);

                logger.LogInformation("Status: {StatusCode}, body: {Body}", response.StatusCode, body);

                await _channel.BasicAckAsync(ea.DeliveryTag, false, ct);
            };

            await _channel.BasicConsumeAsync(
                opt.CurrentValue.BatchProcessedNotifyQueue,
                false,
                consumer,
                ct);
        }
        catch (Exception e)
        {
            throw new PermanentProcessingException(e.Message);
        }
    }
}