using Pixario.Ingest.Core.Domain;

namespace Pixario.Ingest.Application.Ports.Repositories;

public interface IBatchRepository
{
    Task<RetouchBatch?> GetAsync(Guid id, CancellationToken ct);
    Task<RetouchBatch?> GetProcessing();
    Task AddAsync(RetouchBatch batch, CancellationToken ct);
    Task UpdateAsync(RetouchBatch batch, CancellationToken ct);
}