namespace Pixario.Ingest.Application.Messages;

public record BatchProcessedMessage(
    Guid BatchId
);