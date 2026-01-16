using System.Net.Mime;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Pixario.Ingest.Api.Security;

public sealed class ApiKeyMiddleware(IOptions<ApiKeyOptions> options) : IMiddleware
{
    private readonly ApiKeyOptions _opt = options.Value;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (_opt.ApiKeys.Length == 0)
        {
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError,
                "security_misconfigured", "API key security is not configured.");
            return;
        }

        if (!context.Request.Headers.TryGetValue(_opt.HeaderName, out var provided) ||
            string.IsNullOrWhiteSpace(provided))
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized,
                "missing_api_key", $"Missing required header '{_opt.HeaderName}'.");
            return;
        }

        var providedKey = provided.ToString().Trim();
        var ok = _opt.ApiKeys.Any(k => string.Equals(k, providedKey, StringComparison.Ordinal));

        if (!ok)
        {
            await WriteProblemAsync(context, StatusCodes.Status403Forbidden,
                "invalid_api_key", "Invalid API key.");
            return;
        }

        await next(context);
    }

    private static async Task WriteProblemAsync(HttpContext ctx, int status, string code, string detail)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = MediaTypeNames.Application.Json;

        var payload = new
        {
            type = "about:blank",
            title = status switch
            {
                401 => "Unauthorized",
                403 => "Forbidden",
                _ => "Error"
            },
            status,
            code,
            detail,
            traceId = ctx.TraceIdentifier
        };

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
