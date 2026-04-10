using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Core.Entities;

public class ImageRetouchJob
{
    public ImageRetouchJob()
    {
    }

    public ImageRetouchJob(Guid batchId, ImageAsset image)
    {
        Id = Guid.CreateVersion7();
        BatchId = batchId;
        Image = image;
        Status = JobStatus.Queued;
    }

    public Guid Id { get; init; }
    public Guid BatchId { get; init; }
    public ImageAsset Image { get; init; }
    public Guid ImageId { get; init; }
    public JobStatus Status { get; private set; }


    public void MarkProcessing()
    {
        Status = JobStatus.Processing;
    }

    public void MarkDone()
    {
        Status = JobStatus.Done;
    }

    public void MarkFailed()
    {
        Status = JobStatus.Failed;
    }
}