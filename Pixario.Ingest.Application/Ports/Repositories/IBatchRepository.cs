using Pixario.Ingest.Core.Entities;

namespace Pixario.Ingest.Application.Ports.Repositories;

public interface IBatchRepository
{
    Task<ImageRetouchBatch?> GetAsync(Guid id, CancellationToken ct);
    Task<ImageRetouchBatch?> GetProcessing();
    Task AddAsync(ImageRetouchBatch batch, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
}