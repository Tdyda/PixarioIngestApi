using Microsoft.EntityFrameworkCore;
using Pixario.Ingest.Application.Ports;
using Pixario.Ingest.Core.Entities;

namespace Pixario.Ingest.Infrastructure.Persistence;

public class EfJobRepository(IngestDbContext db) : IJobRepository
{
    private readonly IngestDbContext _db = db;


    public async Task SaveAsync(UploadJob job, CancellationToken ct)
    {
        _db.UploadJobs.Add(job);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<UploadJob?> GetAsync(Guid jobId, CancellationToken ct)
    {
        return await _db.UploadJobs
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.JobId == jobId, ct);
    }

    public async Task UpdateAsync(UploadJob job, CancellationToken ct)
    {
        _db.UploadJobs.Update(job);
        await _db.SaveChangesAsync(ct);
    }
}