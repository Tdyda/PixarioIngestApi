using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Infrastructure.Persistence.Models;

public class ImageRetouchBatch
{
    [Obsolete("Only for EF", true)]
    private ImageRetouchBatch()
    {
    }

    public ImageRetouchBatch(Guid id, IReadOnlyCollection<ImageRetouchJob> jobs, IReadOnlyCollection<ImageAsset> images)
    {
        Id = id;
        Jobs = jobs;
        Images = images;
        CreatedAt = DateTime.UtcNow;
        Status = JobStatus.Queued;
    }

    public Guid Id { get; private set; }
    public IReadOnlyCollection<ImageRetouchJob> Jobs { get; private set; } = null!;
    public JobStatus Status { get; set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<ImageAsset> Images { get; private set; } = null!;
}