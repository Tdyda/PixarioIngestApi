using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Infrastructure.Persistence.Models;

public class ImageRetouchJob
{
    public ImageRetouchJob()
    {
    }

    public ImageRetouchJob(Guid id, Guid batchId, Guid imageId, JobStatus status)
    {
        Id = id;
        BatchId = batchId;
        ImageId = imageId;
        Status = status;
    }

    public Guid Id { get; init; }
    public Guid BatchId { get; init; }
    public ImageAsset Image { get; init; } = null!;
    public Guid ImageId { get; init; }
    public JobStatus Status { get; set; }
}