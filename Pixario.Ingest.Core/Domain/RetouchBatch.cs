using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Core.Domain;

public sealed record RetouchBatch
{
    private readonly List<Image> _images = [];
    private readonly List<RetouchJob> _jobs = [];

    private RetouchBatch(Guid id)
    {
        Id = id;
    }

    private RetouchBatch(
        Guid id,
        IReadOnlyCollection<RetouchJob> jobs,
        JobStatus status,
        IReadOnlyCollection<Image> images)
    {
        Id = id;
        Status = status;
        _jobs.AddRange(jobs);
        _images.AddRange(images);
    }

    public Guid Id { get; }
    public IReadOnlyCollection<RetouchJob> Jobs => _jobs;
    public IReadOnlyCollection<Image> Images => _images;
    public JobStatus Status { get; private set; }

    public void AddImage(Image image)
    {
        _images.Add(image);
    }

    public void AddJob(RetouchJob job)
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

    public RetouchJob? GetNextPendingJob()
    {
        return _jobs.FirstOrDefault(j => j.Status == JobStatus.Queued);
    }

    public IReadOnlyCollection<RetouchJob> GetAllJobs()
    {
        return _jobs;
    }

    public static RetouchBatch Create(Guid id)
    {
        return new RetouchBatch(id);
    }

    public static RetouchBatch Create(
        Guid id,
        IReadOnlyCollection<RetouchJob> jobs,
        JobStatus status,
        IReadOnlyCollection<Image> images)
    {
        return new RetouchBatch(id, jobs, status, images);
    }
}