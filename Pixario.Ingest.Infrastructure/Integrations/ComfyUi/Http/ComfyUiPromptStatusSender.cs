using System.Text.Json;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Extensions;
using Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Configuration;

namespace Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Http;

public class ComfyUiPromptStatusSender(
    HttpClient httpClient,
    IOptionsMonitor<ComfyUiConfig> comfyOpt)
{
    public async Task<JsonDocument?> SendAsync(string promptId, CancellationToken ct)
    {
        var resp = await httpClient.GetAsync($"{comfyOpt.CurrentValue.Url}/history/{promptId}", ct);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync(ct);
        var historyDoc = JsonDocument.Parse(json);

        return historyDoc.HasNonEmptyOutputs() ? historyDoc : null;
    }
}