using Pixario.Ingest.Application.Ports.Logging;
using Pixario.Ingest.Core.Domain;

namespace Pixario.Ingest.Application.Features.Logging.Get;

public class GetLogLevelHandler(ILogLevelRepository repository)
{
    public async Task<LogLevel?> Handle(CancellationToken ct)
    {
        return await repository.GetAsync(ct);
    }
}