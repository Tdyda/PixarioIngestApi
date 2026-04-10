namespace Pixario.Ingest.Core.Entities;

public class ImageAsset
{
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

    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string StoragePath { get; set; }
    public long Size { get; set; }
    public Guid BatchId { get; set; }
}