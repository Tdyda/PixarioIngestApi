namespace Pixario.Ingest.Application.Ports;

public interface IImageProcessingPipeline
{
    Task<IReadOnlyList<string>> RunAsync(PipelineRequest request, CancellationToken ct);
}

public sealed record PipelineRequest(
    Guid JobId,
    IReadOnlyList<string> InputFiles
);