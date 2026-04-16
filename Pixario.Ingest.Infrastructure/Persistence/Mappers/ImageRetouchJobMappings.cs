using Pixario.Ingest.Core.Domain;
using Pixario.Ingest.Infrastructure.Persistence.Models;

namespace Pixario.Ingest.Infrastructure.Persistence.Mappers;

public static class ImageRetouchJobMappings
{
    public static ImageRetouchJob Map(this RetouchJob job)
    {
        return new ImageRetouchJob(job.Id, job.BatchId, job.Image.Map(), job.Status);
    }

    public static RetouchJob Map(this ImageRetouchJob job)
    {
        return RetouchJob.Create(job.Id, job.BatchId, job.Image.Map(), job.Status);
    }
}