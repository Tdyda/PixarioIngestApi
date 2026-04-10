using Pixario.Ingest.Application.Messages;

namespace Pixario.Ingest.Application.Ports.Messaging;

public interface IBatchProcessedNotifier
{
    public Task PublishAsync(BatchProcessedMessage message, CancellationToken ct);
}