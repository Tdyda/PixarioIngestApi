namespace Pixario.Ingest.Infrastructure.Persistence.Models;

public class ImageAsset
{
    [Obsolete("Only for EF", true)]
    private ImageAsset()
    {
    }

    public ImageAsset(Guid id, string originalFileName, Guid storedFileName, string storagePath, long size)
    {
        Id = id;
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        StoragePath = storagePath;
        Size = size;
    }

    public Guid Id { get; init; }
    public string OriginalFileName { get; init; } = null!;
    public Guid StoredFileName { get; } = Guid.Empty;
    public string StoragePath { get; init; } = null!;
    public long Size { get; init; }
}