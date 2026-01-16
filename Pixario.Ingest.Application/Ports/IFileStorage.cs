namespace Pixario.Ingest.Application.Ports;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string fileName, Guid jobId, CancellationToken ct);
}