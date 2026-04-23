namespace Pixario.Ingest.Core.Domain;

public class Image
{
    private Image(Guid id, string originalFileName, Guid storedFileName, string storagePath, long size)
    {
        Id = id;
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        StoragePath = storagePath;
        Size = size;
    }

    public Guid Id { get; set; }
    public string OriginalFileName { get; set; }
    public Guid StoredFileName { get; set; }
    public string StoragePath { get; set; }
    public long Size { get; set; }
    public Guid BatchId { get; set; }

    public static Image Create(Guid id, string originalFileName, Guid storedFileName, string storagePath, long size)
    {
        return new Image(id, originalFileName, storedFileName, storagePath, size);
    }
}