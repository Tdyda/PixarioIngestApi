namespace Pixario.Ingest.Application.Features.Worker.ImageProcessing;

public sealed class CheckImageStatusErrorMapper
{
    public void Map(CheckImageStatusDto response)
    {
        var exceptionMessage = response.ExceptionMessage?.Trim();

        if (string.IsNullOrWhiteSpace(exceptionMessage))
            return;

        if (exceptionMessage.Contains("list index out of range", StringComparison.OrdinalIgnoreCase)
            && ContainsIgnoreCase(response.NodeType, "BBoxListItemSelect")
            && ContainsIgnoreCase(response.NodeType, "FaceParsing"))
        {
            response.SetExceptionMessage("No face detected");
            return;
        }

        response.SetExceptionMessage(exceptionMessage);
    }

    private static bool ContainsIgnoreCase(string? source, string value)
    {
        return !string.IsNullOrWhiteSpace(source)
               && source.Contains(value, StringComparison.OrdinalIgnoreCase);
    }
}