using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports.Messaging;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;

namespace Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Publishing;

public class BatchProcessedNotifier(
    RabbitMqPublisherSupport publisherSupport,
    IOptionsMonitor<RabbitMqOptions> opt) : IBatchProcessedNotifier
{
    public async Task PublishAsync(BatchProcessedMessage message, CancellationToken ct)
    {
        await publisherSupport.PublishAsync(
            message,
            async (channel, body, props, token) =>
            {
                await channel.BasicPublishAsync(
                    opt.CurrentValue.Exchange,
                    opt.CurrentValue.BatchProcessedNotifyRoutingKey,
                    false,
                    props,
                    body,
                    token);
            }, ct);
    }
}