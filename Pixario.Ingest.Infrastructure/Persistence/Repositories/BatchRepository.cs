using Microsoft.EntityFrameworkCore;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Core.Entities;
using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Infrastructure.Persistence.Repositories;

public class BatchRepository(IngestDbContext db) : IBatchRepository
{
    public async Task<ImageRetouchBatch?> GetAsync(Guid id, CancellationToken ct)
    {
        return await db.ImageRetouchBatches
            .Where(b => b.Id == id)
            .Include(b => b.Jobs)
            .Include(b => b.Images)
            .FirstOrDefaultAsync(ct);
    }


    public async Task<ImageRetouchBatch?> GetProcessing()
    {
        return await db.ImageRetouchBatches
            .FirstOrDefaultAsync(b => b.Status == JobStatus.Processing);
    }

    public async Task AddAsync(ImageRetouchBatch batch, CancellationToken ct)
    {
        await db.ImageRetouchBatches.AddAsync(batch, ct);
    }

    public async Task SaveAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}