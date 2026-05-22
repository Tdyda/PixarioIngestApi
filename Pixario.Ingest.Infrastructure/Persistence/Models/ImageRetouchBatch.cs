using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Infrastructure.Persistence.Models;

public class ImageRetouchBatch
{
    [Obsolete("Only for EF", true)]
    private ImageRetouchBatch()
    {
    }

    public ImageRetouchBatch(Guid id, IReadOnlyCollection<ImageRetouchJob> jobs, Guid galleryId, string? batchFailedReason = null)
    {
        Id = id;
        Jobs = jobs;
        Status = JobStatus.Queued;
        GalleryId = galleryId;
        BatchFailedReason = batchFailedReason;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public IReadOnlyCollection<ImageRetouchJob> Jobs { get; private set; } = null!;
    public JobStatus Status { get; set; }
    public Guid GalleryId { get; }
    public string? BatchFailedReason { get; set; }
    public DateTime CreatedAt { get; private set; }
}