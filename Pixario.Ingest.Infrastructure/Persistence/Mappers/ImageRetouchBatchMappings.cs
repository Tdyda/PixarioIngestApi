using Pixario.Ingest.Core.Domain;
using Pixario.Ingest.Infrastructure.Persistence.Models;

namespace Pixario.Ingest.Infrastructure.Persistence.Mappers;

public static class ImageRetouchBatchMappings
{
    public static ImageRetouchBatch Map(this RetouchBatch domain)
    {
        var jobs = domain.GetAllJobs().Select(j => j.Map()
            )
            .ToList();

        return new ImageRetouchBatch(domain.Id, jobs, domain.GalleryId, domain.BatchFailedReason);
    }

    public static RetouchBatch Map(this ImageRetouchBatch model)
    {
        var jobs = model.Jobs.Select(j => j.Map()
            )
            .ToList();

        return RetouchBatch.Create(model.Id, jobs, model.Status, model.GalleryId, model.BatchFailedReason);
    }
}