namespace Pixario.Ingest.Core.Domain;

public class Image
{
    private Image(Guid id, string fileName, string storagePath, long size)
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

    public static Image Create(Guid id, string fileName, string storagePath, long size)
    {
        return new Image(id, fileName, storagePath, size);
    }
}