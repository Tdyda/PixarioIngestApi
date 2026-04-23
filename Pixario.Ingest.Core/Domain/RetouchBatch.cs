using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Core.Domain;

public sealed record RetouchBatch
{
    private readonly List<RetouchJob> _jobs = [];

    private RetouchBatch(Guid id)
    {
        Id = id;
    }

    private RetouchBatch(
        Guid id,
        IReadOnlyCollection<RetouchJob> jobs,
        JobStatus status,
        string? batchFailedReason = null)
    {
        Id = id;
        Status = status;
        _jobs.AddRange(jobs);
        BatchFailedReason = batchFailedReason;
    }

    public Guid Id { get; }
    public IReadOnlyCollection<RetouchJob> Jobs => _jobs;
    public JobStatus Status { get; private set; }
    public string? BatchFailedReason { get; set; }

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
        string? batchFailedReason = null)
    {
        return new RetouchBatch(id, jobs, status, batchFailedReason);
    }
}