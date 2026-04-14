using Serilog.Core;
using Serilog.Events;

namespace Pixario.Ingest.Api.HostedServices;

public class LogLevelService(LoggingLevelSwitch @switch)
{
    public void SetDebug() => @switch.MinimumLevel = LogEventLevel.Debug;
    public void SetInfo() => @switch.MinimumLevel = LogEventLevel.Information;
    public void SetWarn() => @switch.MinimumLevel = LogEventLevel.Warning;
    public void SetError() => @switch.MinimumLevel = LogEventLevel.Error;
}