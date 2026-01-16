using Pixario.Ingest.Application.Messages;

namespace Pixario.Ingest.Application.Ports;

public interface IQueuePublisher
{
    Task PublishAsync(ImageProcessingMessage message, CancellationToken token);
}