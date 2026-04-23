using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Infrastructure.Persistence.Models;

public class ImageRetouchJob
{
    public ImageRetouchJob()
    {
    }

    public ImageRetouchJob(Guid id, Guid batchId, ImageAsset image, JobStatus status, string? jobFailedReason = null)
    {
        Id = id;
        BatchId = batchId;
        Image = image;
        ImageId = image.Id;
        Status = status;
        JobFailedReason = jobFailedReason;
    }

    public Guid Id { get; init; }
    public Guid BatchId { get; init; }
    public ImageAsset Image { get; init; } = null!;
    public Guid ImageId { get; init; }
    public JobStatus Status { get; set; }
    public string? JobFailedReason { get; set; }
}