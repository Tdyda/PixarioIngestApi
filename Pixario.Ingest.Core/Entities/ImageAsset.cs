namespace Pixario.Ingest.Core.Entities;

public class ImageAsset
{
    private ImageAsset()
    {
    }

    public ImageAsset(Guid imageId, string fileName, string storagePath, long size)
    {
        ImageId = imageId;
        FileName = fileName;
        StoragePath = storagePath;
        Size = size;
    }

    public Guid ImageId { get; set; }
    public string FileName { get; set; }
    public string StoragePath { get; set; }
    public long Size { get; set; }

    public Guid JobId { get; set; }
}