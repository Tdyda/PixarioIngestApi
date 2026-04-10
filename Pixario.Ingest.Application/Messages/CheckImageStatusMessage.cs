using Pixario.Ingest.Core.Entities;

namespace Pixario.Ingest.Application.Messages;

public class CheckImageStatusMessage
{
    public Guid BatchId { get; set; }
    public ImageRetouchJob Job { get; set; }
    public required string PromptId { get; init; }
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
}