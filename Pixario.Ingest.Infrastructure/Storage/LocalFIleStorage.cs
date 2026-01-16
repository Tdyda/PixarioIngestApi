using Pixario.Ingest.Application.Ports;

namespace Pixario.Ingest.Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(string root)
    {
        _root = root;
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(
        Stream content,
        string fileName,
        Guid jobId,
        CancellationToken ct)
    {
        var dir = Path.Combine(_root, jobId.ToString());
        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, fileName);

        await using var fs = File.Create(path);
        await content.CopyToAsync(fs, ct);

        return path;
    }
}
