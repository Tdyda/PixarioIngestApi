using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Core.Entities;

public class ImageRetouchBatch
{
    private readonly List<ImageAsset> _images = [];
    private readonly List<ImageRetouchJob> _jobs = [];

    private ImageRetouchBatch()
    {
    }

    public ImageRetouchBatch(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
        Status = JobStatus.Queued;
    }

    public Guid Id { get; private set; }
    public IReadOnlyCollection<ImageRetouchJob> Jobs => _jobs;
    public JobStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<ImageAsset> Images => _images;

    public void AddImage(ImageAsset image)
    {
        _images.Add(image);
    }

    public void AddJob(ImageRetouchJob job)
    {
        _jobs.Add(job);
    }

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

    public ImageRetouchJob? GetNextPendingJob()
    {
        return Jobs.FirstOrDefault(j => j.Status == JobStatus.Queued);
    }
}