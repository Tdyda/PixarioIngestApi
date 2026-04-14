using Microsoft.Extensions.Logging;
using Pixario.Ingest.Application.Ports.Logging;
using LogLevel = Pixario.Ingest.Core.Domain.LogLevel;

namespace Pixario.Ingest.Application.Features.Logging.Update;

public class UpdateLogLevelHandler(
    ILogLevelRepository repository,
    ILogger<UpdateLogLevelHandler> logger)
{
    public async Task Handle(LogLevel level, CancellationToken ct)
    {
        try
        {
            await repository.UpdateAsync(level, ct);
            logger.LogInformation("Updated log level: {level}", level);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Invalid operation while updating log level");
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex, "Log level not found: {Level}", level);
        }
    }
}