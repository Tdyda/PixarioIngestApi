using Pixario.Ingest.Core.Domain;
using Pixario.Ingest.Infrastructure.Persistence.Models;

namespace Pixario.Ingest.Infrastructure.Persistence.Mappers;

public static class ImageAssetMappings
{
    public static Image Map(this ImageAsset model)
    {
        return Image.Create(model.Id, model.FileName, model.StoragePath, model.Size);
    }

    public static ImageAsset Map(this Image domain)
    {
        return new ImageAsset(domain.Id, domain.FileName, domain.StoragePath, domain.Size);
    }
}