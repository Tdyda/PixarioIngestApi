using System.Collections.Concurrent;
using Pixario.Ingest.Application.Ports;
using Pixario.Ingest.Core.Entities;

namespace Pixario.Ingest.Infrastructure.Persistence;

public class InMemoryJobRepository : IJobRepository
{
    private static readonly ConcurrentDictionary<Guid, UploadJob> Store = new();
    public Task SaveAsync(UploadJob job, CancellationToken ct)
    {
        Store[job.JobId] = job;
        return Task.CompletedTask;
    }

    public Task<UploadJob?> GetAsync(Guid jobId, CancellationToken ct)
    {
        Store.TryGetValue(jobId, out var job);
        return Task.FromResult(job);
    }

    public Task UpdateAsync(UploadJob job, CancellationToken ct)
    {
        Store[job.JobId] = job;
        return Task.CompletedTask;
    }
}