using Microsoft.EntityFrameworkCore;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Core.Domain;
using Pixario.Ingest.Core.Enums;
using Pixario.Ingest.Infrastructure.Persistence.Mappers;

namespace Pixario.Ingest.Infrastructure.Persistence.Repositories;

public class JobRepository(IngestDbContext db) : IJobRepository
{
    public async Task SaveAsync(RetouchJob job, CancellationToken ct)
    {
        await db.ImageRetouchJobs.AddAsync(job.Map(), ct);
    }

    public async Task<RetouchJob?> GetAsync(Guid jobId, CancellationToken ct)
    {
        var model = await db.ImageRetouchJobs
            .AsNoTracking()
            .Include(x => x.Image)
            .FirstOrDefaultAsync(x => x.Id == jobId, ct);

        return model?.Map();
    }

    public async Task UpdateAsync(RetouchJob job, CancellationToken ct)
    {
        var entity = await db.ImageRetouchJobs
            .FirstAsync(x => x.Id == job.Id, ct);

        entity.Status = job.Status;
        entity.JobFailedReason = job.JobFailedReason;
    }

    public async Task<IReadOnlyList<RetouchJob>> GetProcessedAsync(Guid batchId, CancellationToken ct)
    {
        return await db.ImageRetouchJobs
            .Where(x => x.BatchId == batchId && x.Status == JobStatus.Processing)
            .Select(j => j.Map())
            .ToListAsync(ct);
    }
}