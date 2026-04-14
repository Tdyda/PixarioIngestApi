using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Core.Domain;

public class RetouchJob
{
    private RetouchJob(Guid id, Guid batchId, Image image, JobStatus status)
    {
        Id = id;
        BatchId = batchId;
        Image = image;
        Status = status;
    }

    public Guid Id { get; init; }
    public Guid BatchId { get; init; }
    public Image Image { get; init; }
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

    public static RetouchJob Create(Guid id, Guid batchId, Image image, JobStatus status)
    {
        return new RetouchJob(id, batchId, image, status);
    }
}