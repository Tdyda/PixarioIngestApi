using Pixario.Ingest.Application.Ports.Repositories;

namespace Pixario.Ingest.Infrastructure.Persistence.Repositories;

public class UnitOfWork(IngestDbContext db) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}