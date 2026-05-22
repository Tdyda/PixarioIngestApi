using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Ports.Storage;
using Pixario.Ingest.Core.Domain;
using Pixario.Ingest.Core.Enums;
using Pixario.Ingest.Infrastructure.Exceptions;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Configuration;

namespace Pixario.Ingest.Infrastructure.Integrations.Outbound.Builders;

public class PixarioBatchProcessedPayloadBuilder(
    IFileStorage storage,
    IOptionsMonitor<PixarioOptions> opt)
{
    private readonly MultipartFormDataContent _content = new();

    public Task<HttpRequestMessage> BuildAsync()
    {
        var req = new HttpRequestMessage();
        req.Content = _content;
        req.Method = HttpMethod.Post;
        req.Headers.TryAddWithoutValidation("Cookie", $"x-api-key={opt.CurrentValue.ApiKey}");

        return Task.FromResult(req);
    }

    public async Task<PixarioBatchProcessedPayloadBuilder> AddFilesAsync(RetouchBatch batch)
    {
        if (batch.Jobs.All(j => j.Status != JobStatus.Done))
            throw new ExternalServiceBadRequestException("No files to upload.");

        foreach (var job in batch.Jobs)
            if (job.Status == JobStatus.Done)
            {
                var stream = await storage.LoadFile(job.Image.StoredFileName.ToString());
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType =
                    new MediaTypeHeaderValue(GetContentType(job.Image.StoredFileName.ToString()));

                _content.Add(streamContent, "files[]", job.Image.OriginalFileName);
            }

        return this;
    }

    public Task<PixarioBatchProcessedPayloadBuilder> AddResultsMapAsync(RetouchBatch batch)
    {
        _content.Add(new StringContent(batch.GalleryId.ToString()), "galleryId");
        
        List<FileProcessResult> results = [];
        results.AddRange(
            batch.Jobs.Select(job =>
                new FileProcessResult(
                    job.Image.OriginalFileName,
                    job.Status.ToString(),
                    job.JobFailedReason)
            ));

        var jsonResult = JsonSerializer.Serialize(results);
        var resultsContent = new StringContent(jsonResult, Encoding.UTF8, "application/json");
        _content.Add(resultsContent, "results");

        return Task.FromResult(this);
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