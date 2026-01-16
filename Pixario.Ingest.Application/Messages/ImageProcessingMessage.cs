namespace Pixario.Ingest.Application.Messages;

public class ImageProcessingMessage
{
    public Guid JobId { get; init; }
    public IReadOnlyCollection<string> Files { get; init; } = [];
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}