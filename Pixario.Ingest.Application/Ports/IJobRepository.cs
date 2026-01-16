using Pixario.Ingest.Core.Entities;

namespace Pixario.Ingest.Application.Ports;

public interface IJobRepository
{
    Task SaveAsync(UploadJob job, CancellationToken ct);
    Task<UploadJob?> GetAsync(Guid jobId, CancellationToken ct);
    Task UpdateAsync(UploadJob job, CancellationToken ct);
}