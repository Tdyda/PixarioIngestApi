using Microsoft.EntityFrameworkCore;
using Pixario.Ingest.Application.Ports.Logging;
using Pixario.Ingest.Core.Domain;

namespace Pixario.Ingest.Infrastructure.Persistence.Repositories;

public class LogLevelRepository(
    IngestDbContext context
    ) : ILogLevelRepository
{
    public async Task<LogLevel?> GetAsync(CancellationToken ct)
    {
        return await context.LogLevels
            .Where(l => l.IsActive)
            .Select(l => new LogLevel(l.LogLevel))
            .SingleOrDefaultAsync(ct);
    }

    public async Task UpdateAsync(LogLevel level, CancellationToken ct)
    {
        var currentLevel = await context.LogLevels
            .SingleOrDefaultAsync(l => l.IsActive, ct);

        var nextLevel = await context.LogLevels
            .SingleOrDefaultAsync(l => l.LogLevel == level.Level, ct);

        if (nextLevel is null)
        {
            throw new InvalidOperationException($"Log level '{level.Level}' does not exist.");
        }

        if (currentLevel is not null && currentLevel.Id != nextLevel.Id)
        {
            currentLevel.IsActive = false;
        }

        nextLevel.IsActive = true;
    }
}