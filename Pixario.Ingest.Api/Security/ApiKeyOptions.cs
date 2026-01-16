namespace Pixario.Ingest.Api.Security;

public class ApiKeyOptions
{
    public const string SectionName = "Security";

    public string HeaderName { get; init; } = "X-API-KEY";
    public string[] ApiKeys { get; init; } = Array.Empty<string>();
}