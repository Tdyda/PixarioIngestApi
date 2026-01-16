using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports;

namespace Pixario.Ingest.Infrastructure;

public class FakeQueuePublisher : IQueuePublisher
{
    public Task PublishAsync(ImageProcessingMessage message, CancellationToken ct)
        => Task.CompletedTask;
}