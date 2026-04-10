using Pixario.Ingest.Application.Messages;

namespace Pixario.Ingest.Application.Ports.Messaging;

public interface ICheckImageProcessingStatusPublisher
{
    public Task PublishAsync(CheckImageStatusMessage message, CancellationToken ct);
}