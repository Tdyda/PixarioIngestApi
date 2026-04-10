namespace Pixario.Ingest.Application.Features.Api.Jobs.CreateBatch;

public sealed record CreateJobCommand(IEnumerable<JobInputFile> Files);