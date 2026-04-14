using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Pixario.Ingest.Api.Extensions;

public static class LoggingExtensions
{
    public static IHostApplicationBuilder AddPixarioLogging(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var appLevelSwitch = new LoggingLevelSwitch();
        builder.Services.AddSingleton(appLevelSwitch);
        
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.ControlledBy(appLevelSwitch)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.Logger(lc => lc
                .Filter.ByExcluding(e =>
                    e.Properties.TryGetValue("SourceContext", out var sc) &&
                    (
                        sc.ToString().Contains("Microsoft") ||
                        sc.ToString().Contains("System")
                    ))
                .WriteTo.File(
                    path: "logs/app-files-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30))
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e =>
                    e.Properties.TryGetValue("SourceContext", out var sc) &&
                    (
                        sc.ToString().Contains("Microsoft") ||
                        sc.ToString().Contains("System")
                    ))
                .WriteTo.File(
                    path: "logs/server-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14))
            .WriteTo.Console()
            .CreateLogger();
        
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger, dispose: true);

        return builder;
    }
}