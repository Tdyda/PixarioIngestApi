using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Ports.Integrations;
using Pixario.Ingest.Core.Domain;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Builders;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Configuration;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Extensions;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Http;

namespace Pixario.Ingest.Infrastructure.Integrations.Outbound.Clients;

public class PixarioBatchProcessedGateway(
    PixarioBatchProcessedPayloadBuilder builder,
    PixarioBatchProcessedCallbackSender sender,
    IOptionsMonitor<PixarioOptions> opt) : IPixarioBatchProcessedGateway
{
    public async Task<HttpResponseMessage> ProcessAsync(RetouchBatch batch, CancellationToken ct)
    {
        var req = await builder
            .AddFilesAsync(batch)
            .AddResultsMapAsync(batch)
            .BuildAsync();

        return await sender.SendAsync(req, opt.CurrentValue.BaseUrl, opt.CurrentValue.FilesProcessedSuccessfully, ct);
    }

    public async Task<HttpResponseMessage> ProcessDlqAsync(RetouchBatch batch, CancellationToken ct)
    {
        var req = await builder
            .AddResultsMapAsync(batch)
            .BuildAsync();
        
        return await sender.SendAsync(req, opt.CurrentValue.BaseUrl, opt.CurrentValue.FilesProcessingFailed, ct);
    }
}