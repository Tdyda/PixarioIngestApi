using Microsoft.Extensions.Options;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Configuration;

namespace Pixario.Ingest.Infrastructure.Integrations.Outbound.Http;

public class PixarioBatchProcessedCallbackSender(
    HttpClient httpClient,
    IOptionsMonitor<PixarioOptions> opt)
{
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        request.RequestUri = new Uri($"{opt.CurrentValue.BaseUrl}/{opt.CurrentValue.EndpointPath}");

        var response = await httpClient.SendAsync(request, ct);

        return response;
    }
}