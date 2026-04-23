using Pixario.Ingest.Core.Domain;

namespace Pixario.Ingest.Application.Ports.Repositories;

public interface IJobRepository
{
    Task SaveAsync(RetouchJob job, CancellationToken ct);
    Task<RetouchJob?> GetAsync(Guid jobId, CancellationToken ct);
    Task UpdateAsync(RetouchJob job, CancellationToken ct);
    Task<IReadOnlyList<RetouchJob>> GetProcessedAsync(Guid batchId, CancellationToken ct);
}