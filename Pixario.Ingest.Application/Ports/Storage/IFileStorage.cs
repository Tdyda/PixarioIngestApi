namespace Pixario.Ingest.Application.Ports.Storage;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct);
    void RenameFile(string oldName, string newName);
}