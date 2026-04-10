using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports.Messaging;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;

namespace Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Publishing;

public class CheckImageProcessingStatusPublisher(
    IOptionsMonitor<RabbitMqOptions> opt,
    RabbitMqPublisherSupport publisherSupport) : ICheckImageProcessingStatusPublisher
{
    public async Task PublishAsync(
        CheckImageStatusMessage message,
        CancellationToken ct)
    {
        await publisherSupport.PublishAsync(
            message,
            async (channel, body, props, token) =>
            {
                await channel.BasicPublishAsync(
                    opt.CurrentValue.Exchange,
                    opt.CurrentValue.ImageStatusCheckRoutingKey,
                    false,
                    props,
                    body,
                    token);
            }, ct);
    }
}