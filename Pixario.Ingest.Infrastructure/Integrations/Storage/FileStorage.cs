using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Ports.Storage;
using Pixario.Ingest.Infrastructure.Integrations.Storage.Configuration;

namespace Pixario.Ingest.Infrastructure.Integrations.Storage;

public class FileStorage(
    IOptionsMonitor<FileStorageOptions> opt) : IFileStorage
{
    public async Task<string> SaveAsync(
        Stream content,
        string fileName,
        CancellationToken ct)
    {
        var path = Path.Combine(opt.CurrentValue.InputDir, fileName);

        await using var fs = File.Create(path);
        await content.CopyToAsync(fs, ct);

        return path;
    }

    public Task<Stream> LoadFile(string fileName)
    {
        var path = Path.Combine(opt.CurrentValue.OutputDir, fileName);

        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found at path: {path}");

        Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        return Task.FromResult(stream);
    }

    public void RenameFile(string oldName, string newName)
    {
        var path = Path.Combine(opt.CurrentValue.OutputDir, oldName);
        File.Move(path, Path.Combine(opt.CurrentValue.OutputDir, newName));
    }
}