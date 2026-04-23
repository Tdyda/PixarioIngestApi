using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Exceptions;
using Pixario.Ingest.Application.Features.Worker.ImageProcessing;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Core.Domain;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pixario.Ingest.Worker.Messaging;

public class CheckImageStatusDlqConsumer(
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
            RetouchJob? job = null;
            using var scope = scopeFactory.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg = JsonSerializer.Deserialize<CheckImageStatusMessage>(json);

                if (msg is null)
                    throw new PermanentProcessingException("Invalid completed message payload");
                
                job = await jobRepository.GetAsync(msg.JobId, ct);
                
                if (job is null)
                    throw new PermanentProcessingException(
                        $"Job {msg.JobId} not found in DLQ consumer");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error while handling DLQ message");
            }
            finally
            {
                if (job is not null)
                {
                    job.MarkFailed();
                    await jobRepository.UpdateAsync(job, ct);
                    await unitOfWork.SaveChangesAsync(ct);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false, ct);
            }
        };
        
        await _channel.BasicConsumeAsync(
            opt.CurrentValue.ImageStatusCheckDlqQueue,
            false,
            consumer,
            ct);
    }
}