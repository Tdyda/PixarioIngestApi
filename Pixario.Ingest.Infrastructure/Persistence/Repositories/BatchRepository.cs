using Microsoft.EntityFrameworkCore;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Core.Domain;
using Pixario.Ingest.Core.Enums;
using Pixario.Ingest.Infrastructure.Persistence.Mappers;

namespace Pixario.Ingest.Infrastructure.Persistence.Repositories;

public class BatchRepository(IngestDbContext db) : IBatchRepository
{
    public async Task<RetouchBatch?> GetAsync(Guid id, CancellationToken ct)
    {
        var model = await db.ImageRetouchBatches.Where(b => b.Id == id)
            .AsSplitQuery()
            .AsNoTracking()
            .Include(b => b.Jobs)
            .ThenInclude(j => j.Image)
            .Include(b => b.Images)
            .FirstOrDefaultAsync(ct);

        return model?.Map();
    }


    public async Task<RetouchBatch?> GetProcessing()
    {
        var model = await db.ImageRetouchBatches
            .AsSplitQuery()
            .Include(b => b.Jobs)
            .ThenInclude(j => j.Image)
            .Include(b => b.Jobs)
            .FirstOrDefaultAsync(b => b.Status == JobStatus.Processing);

        return model?.Map();
    }

    public async Task AddAsync(RetouchBatch batch, CancellationToken ct)
    {
        await db.ImageRetouchBatches.AddAsync(batch.Map(), ct);
    }

    public async Task UpdateAsync(RetouchBatch batch, CancellationToken ct)
    {
        var entity = await db.ImageRetouchBatches
            .FirstAsync(x => x.Id == batch.Id, ct);

        entity.Status = batch.Status;
    }
}