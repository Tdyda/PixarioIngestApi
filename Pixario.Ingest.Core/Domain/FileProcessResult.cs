using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Core.Domain;

public record FileProcessResult(string FileName, string Status, string? ErrorMessage);