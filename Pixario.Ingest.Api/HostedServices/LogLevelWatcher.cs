using Pixario.Ingest.Application.Features.Logging.Get;
using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Api.HostedServices;

public class LogLevelWatcher(
    LogLevelService svc,
    IServiceScopeFactory scopeFactory,
    ILogger<LogLevelWatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<GetLogLevelHandler>();
                var level = await handler.Handle(ct);

                if (level is null) return;

                switch (level.Level)
                {
                    case LogLevelValue.Debug:
                        svc.SetDebug();
                        break;
                    case LogLevelValue.Info:
                        svc.SetInfo();
                        break;
                    case LogLevelValue.Warning:
                        svc.SetWarn();
                        break;
                    case LogLevelValue.Error:
                        svc.SetError();
                        break;
                    default:
                        svc.SetInfo();
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error");
            }
            
            await Task.Delay(5000, ct);
        }
    }
}