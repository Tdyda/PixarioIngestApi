using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Configuration;

namespace Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Http;

public class ComfyUiPromptSender(
    HttpClient httpClient,
    IOptionsMonitor<ComfyUiConfig> comfyOpt)
{
    public async Task<string> SendAsync(object payload, CancellationToken ct)
    {
        var url = $"{comfyOpt.CurrentValue.Url}/prompt";

        var response = await httpClient.PostAsJsonAsync(url, payload, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStreamAsync(ct);
        var doc = await JsonDocument.ParseAsync(result, cancellationToken: ct);
        var promptId = doc.RootElement.GetProperty("prompt_id").GetString()
                       ?? throw new InvalidOperationException("Response does not contain prompt_id.");

        return promptId;
    }
}