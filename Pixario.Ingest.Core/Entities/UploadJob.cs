using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Core.Entities;

public class UploadJob
{
    private UploadJob()
    {
    }

    public UploadJob(Guid jobId)
    {
        JobId = jobId;
        CreatedAt = DateTime.UtcNow;
        Status = JobStatus.Queued;
    }

    public Guid JobId { get; private set; }
    public JobStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<ImageAsset> Images => _images;

    private readonly List<ImageAsset> _images = [];

    public void AddImage(ImageAsset image) => _images.Add(image);

    public void MarkProcessing() => Status = JobStatus.Processing;
    public void MarkDone() => Status = JobStatus.Done;
    public void MarkFailed() => Status = JobStatus.Failed;
}