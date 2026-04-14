using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Ports.Storage;
using Pixario.Ingest.Core.Domain;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Configuration;

namespace Pixario.Ingest.Infrastructure.Integrations.Outbound.Builders;

public class PixarioBatchProcessedPayloadBuilder(
    IFileStorage storage,
    IOptionsMonitor<PixarioOptions> opt)
{
    public async Task<HttpRequestMessage> BuildAsync(RetouchBatch batch)
    {
        var content = new MultipartFormDataContent();

        content.Add(new StringContent(batch.Id.ToString()), "batchId");

        foreach (var image in batch.Images)
        {
            var stream = await storage.LoadFile(image.FileName);
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(image.FileName));

            content.Add(streamContent, "files[]", image.FileName);
        }

        var req = new HttpRequestMessage();
        req.Content = content;
        req.Method = HttpMethod.Post;
        req.Headers.TryAddWithoutValidation("Cookie", $"x-api-key={opt.CurrentValue.ApiKey}");

        return req;
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}