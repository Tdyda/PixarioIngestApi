using Pixario.Ingest.Core.Entities;

namespace Pixario.Ingest.Application.Ports.Repositories;

public interface IJobRepository
{
    Task SaveAsync(ImageRetouchJob job, CancellationToken ct);
    Task<ImageRetouchJob?> GetAsync(Guid jobId, CancellationToken ct);
    Task UpdateAsync(ImageRetouchJob job, CancellationToken ct);
    Task<IReadOnlyList<ImageRetouchJob>> GetProcessedAsync(Guid batchId, CancellationToken ct);
}