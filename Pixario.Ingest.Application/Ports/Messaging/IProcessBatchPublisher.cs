using Pixario.Ingest.Application.Messages;

namespace Pixario.Ingest.Application.Ports.Messaging;

public interface IProcessBatchPublisher
{
    Task PublishAsync(ProcessBatchMessage message, CancellationToken token);
}