using Microsoft.AspNetCore.Mvc;
using Pixario.Ingest.Api.OpenApi;
using Pixario.Ingest.Application.Features.Api.Jobs.CreateBatch;

namespace Pixario.Ingest.Api.Features.Jobs;

public static class CreateJobFeature
{
    public static void MapUploadFeatureApiKey(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/uploads/images", async (
                CreateJobRequest request,
                [FromServices] CreateJobCommandHandler handler,
                CancellationToken ct) =>
            {
                var jobId = await handler.Handle(
                    new CreateJobCommand(
                        request.Files.Select(f => new JobInputFile(
                            f.Content,
                            f.FileName,
                            f.Size
                        )),
                        request.GalleryId
                    ), ct);

                return Results.Accepted($"/uploads/images/{jobId}", new { jobId });
            })
            .Accepts<IFormFileCollection>("multipart/form-data")
            .WithGroupName(OpenApiDocs.ApiKey);
    }
}