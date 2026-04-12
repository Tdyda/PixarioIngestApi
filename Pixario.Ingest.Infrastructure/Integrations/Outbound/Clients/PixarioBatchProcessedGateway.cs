using Pixario.Ingest.Application.Ports.Integrations;
using Pixario.Ingest.Core.Entities;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Builders;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Http;

namespace Pixario.Ingest.Infrastructure.Integrations.Outbound.Clients;

public class PixarioBatchProcessedGateway(
    PixarioBatchProcessedPayloadBuilder builder,
    PixarioBatchProcessedCallbackSender sender) : IPixarioBatchProcessedGateway
{
    public async Task<HttpResponseMessage> ProcessAsync(ImageRetouchBatch batch, CancellationToken ct)
    {
        var content = await builder.BuildAsync(batch);
        return await sender.SendAsync(content, ct);
    }
}