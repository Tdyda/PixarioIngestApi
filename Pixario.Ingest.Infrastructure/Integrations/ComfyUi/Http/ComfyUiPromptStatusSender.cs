using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Extensions;
using Pixario.Ingest.Infrastructure.Exceptions;
using Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Configuration;

namespace Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Http;

public class ComfyUiPromptStatusSender(
    HttpClient httpClient,
    IOptionsMonitor<ComfyUiConfig> comfyOpt)
{
    public async Task<JsonDocument?> SendAsync(string promptId, CancellationToken ct)
    {
        var response = await httpClient.GetAsync($"{comfyOpt.CurrentValue.Url}/history/{promptId}", ct);
        
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
        var json = await response.Content.ReadAsStringAsync(ct);
        var historyDoc = JsonDocument.Parse(json);

        return historyDoc.HasNonEmptyOutputs() ? historyDoc : null;
    }
}