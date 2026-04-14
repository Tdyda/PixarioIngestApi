using Pixario.Ingest.Core.Domain;

namespace Pixario.Ingest.Application.Ports.Logging;

public interface ILogLevelRepository
{
    Task<LogLevel?> GetAsync(CancellationToken ct);
    Task UpdateAsync(LogLevel level, CancellationToken ct);
}