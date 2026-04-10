namespace Pixario.Ingest.Api.Features.Jobs;

public sealed record UploadFile(Stream Content, string FileName, long Size);