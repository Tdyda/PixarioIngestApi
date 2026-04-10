namespace Pixario.Ingest.Application.Features.Api.Jobs.CreateBatch;

public record JobInputFile(Stream Content, string FileName, long Size);