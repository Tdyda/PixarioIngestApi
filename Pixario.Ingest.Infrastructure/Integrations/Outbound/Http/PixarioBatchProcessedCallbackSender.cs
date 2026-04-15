using System.Net;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Extensions;
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

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            var body404 = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Endpoint not found (404). Body: {body404}");
        }

        if ((int)response.StatusCode >= 500)
        {
            var body = await response.Content.ReadAsStringAsync(ct);

            throw new ExternalServiceUnavailableException(
                (int)response.StatusCode,
                body);
        }

        response.EnsureSuccessStatusCode();

        return response;
    }
}