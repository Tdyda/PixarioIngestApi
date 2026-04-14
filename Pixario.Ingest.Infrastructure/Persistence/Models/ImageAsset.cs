namespace Pixario.Ingest.Infrastructure.Persistence.Models;

public class ImageAsset
{
    [Obsolete("Only for EF", true)]
    private ImageAsset()
    {
    }

    public ImageAsset(Guid id, string fileName, string storagePath, long size)
    {
        Id = id;
        FileName = fileName;
        StoragePath = storagePath;
        Size = size;
    }

    public Guid Id { get; init; }
    public string FileName { get; init; } = null!;
    public string StoragePath { get; init; } = null!;
    public long Size { get; init; }
    public Guid BatchId { get; init; }
}