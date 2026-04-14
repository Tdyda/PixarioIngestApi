namespace Pixario.Ingest.Application.Ports.Storage;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct);
    Task<Stream> LoadFile(string fileName);
    void RenameFile(string oldName, string newName);
}