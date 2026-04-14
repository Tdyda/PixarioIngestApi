namespace Pixario.Ingest.Application.Extensions;

public class ExternalServiceUnavailableException(int statusCode, string? responseBody)
    : Exception($"External service error: {statusCode}")
{
    public int StatusCode { get; } = statusCode;
    public string? ResponseBody { get; } = responseBody;
}