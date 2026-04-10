using Microsoft.EntityFrameworkCore;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Core.Entities;
using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Infrastructure.Persistence.Repositories;

public class JobRepository(IngestDbContext db) : IJobRepository
{
    public async Task SaveAsync(ImageRetouchJob job, CancellationToken ct)
    {
        db.ImageRetouchJobs.Add(job);
        await db.SaveChangesAsync(ct);
    }

    public async Task<ImageRetouchJob?> GetAsync(Guid jobId, CancellationToken ct)
    {
        return await db.ImageRetouchJobs
            .Include(x => x.Image)
            .FirstOrDefaultAsync(x => x.Id == jobId, ct);
    }

    public async Task UpdateAsync(ImageRetouchJob job, CancellationToken ct)
    {
        db.ImageRetouchJobs.Update(job);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ImageRetouchJob>> GetProcessedAsync(Guid batchId, CancellationToken ct)
    {
        return await db.ImageRetouchJobs
            .Where(x => x.BatchId == batchId && x.Status == JobStatus.Processing)
            .ToListAsync(ct);
    }
}