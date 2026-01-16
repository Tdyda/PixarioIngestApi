using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports;
using Pixario.Ingest.Infrastructure.Messaging;
using Pixario.Ingest.Infrastructure.Pipeline;
using Pixario.Ingest.Worker.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pixario.Ingest.Worker.Messaging;

public sealed class ImageProcessingConsumer(
    IRabbitMqConnection conn,
    IOptions<RabbitMqOptions> options,
    IServiceProvider sp,
    ILogger<ImageProcessingConsumer> log)
    : BackgroundService
{
    private readonly RabbitMqOptions _opt = options.Value;

    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await conn.Connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await RabbitMqTopology.EnsureCreatedAsync(_channel, _opt, stoppingToken);
        await _channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg = JsonSerializer.Deserialize<ImageProcessingMessage>(json);

                if (msg is null)
                    throw new PermanentProcessingException("Invalid message payload");

                await HandleAsync(msg, stoppingToken);

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
            queue: _opt.MainQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);
    }

    private async Task HandleAsync(ImageProcessingMessage msg, CancellationToken ct)
    {
        using var scope = sp.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IJobRepository>();

        var job = await repo.GetAsync(msg.JobId, ct)
            ?? throw new PermanentProcessingException($"Job not found: {msg.JobId}");

        job.MarkProcessing();
        await repo.UpdateAsync(job, ct);

        var pipeline = scope.ServiceProvider.GetRequiredService<IImageProcessingPipeline>();
        
        try
        {
            var path = await pipeline.RunAsync(
                new PipelineRequest(msg.JobId, msg.Files.ToList()),
                ct);
            
            job.MarkDone();
            await repo.UpdateAsync(job, ct);

            log.LogInformation("Processed job {JobId}. FIle saved to {Path}", msg.JobId, path);
        }
        catch (PipelineFailedException ex)
        {
            job.MarkFailed();
            await repo.UpdateAsync(job, ct);

            throw new PermanentProcessingException(
                $"Pipeline failed exitCode={ex.ExitCode}. stderr={ex.Stderr}",
                ex);
        }
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
            exchange: _opt.Exchange,
            routingKey: _opt.DlqRoutingKey,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}
