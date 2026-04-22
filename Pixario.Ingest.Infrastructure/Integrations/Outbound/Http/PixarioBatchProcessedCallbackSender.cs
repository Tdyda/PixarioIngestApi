using System.Net;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Infrastructure.Exceptions;
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

        switch (response.StatusCode)
        {
            case HttpStatusCode.NotFound:
            {
                var body404 = await response.Content.ReadAsStringAsync(ct);
                throw new ExternalServiceBadRequestException($"Endpoint not found (404). Body: {body404}");
            }
            case HttpStatusCode.BadRequest:
            {
                var badRequest = await response.Content.ReadAsStringAsync(ct);
                throw new ExternalServiceBadRequestException($"Bad Request (400). Body: {badRequest}");
            }
            case var status when (int)status >= 500:
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                throw new ExternalServiceUnavailableException(body);
            }
        }

        response.EnsureSuccessStatusCode();
        return response;
    }
}