using Pixario.Ingest.Core.Domain;
using Pixario.Ingest.Infrastructure.Persistence.Models;

namespace Pixario.Ingest.Infrastructure.Persistence.Mappers;

public static class ImageRetouchBatchMappings
{
    public static ImageRetouchBatch Map(this RetouchBatch domain)
    {
        var images = domain.Images.Select(i => i.Map())
            .ToList();

        var jobs = domain.GetAllJobs().Select(j => j.Map()
            )
            .ToList();

        return new ImageRetouchBatch(domain.Id, jobs, images);
    }

    public static RetouchBatch Map(this ImageRetouchBatch model)
    {
        var images = model.Images.Select(i => i.Map())
            .ToList();

        var jobs = model.Jobs.Select(j => j.Map()
            )
            .ToList();

        return RetouchBatch.Create(model.Id, jobs, model.Status, images);
    }
}