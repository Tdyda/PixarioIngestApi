using System.Text.Json;

namespace Pixario.Ingest.Application.Extensions;

public static class JsonDocumentExtensions
{
    public static bool HasNonEmptyOutputs(this JsonDocument? doc)
    {
        if (doc == null)
            return false;

        if (doc.RootElement.ValueKind != JsonValueKind.Object)
            return false;

        return doc.RootElement.EnumerateObject().Any();
    }

    public static JsonElement? GetSection(this JsonElement element, string path)
    {
        var segments = path.Split("__", StringSplitOptions.RemoveEmptyEntries);
        var current = element;

        foreach (var segment in segments)
            if (current.ValueKind == JsonValueKind.Object)
            {
                if (!current.TryGetProperty(segment, out current))
                    return null;
            }
            else if (current.ValueKind == JsonValueKind.Array &&
                     int.TryParse(segment, out var index))
            {
                if (current.GetArrayLength() <= index)
                    return null;

                current = current[index];
            }
            else
            {
                return null;
            }

        return current;
    }
}