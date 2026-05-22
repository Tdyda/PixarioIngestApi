namespace Pixario.Ingest.Application.Messages;

public record CheckImageStatusMessage(
    Guid BatchId,
    Guid JobId,
    string PromptId,
    DateTime CompletedAt);