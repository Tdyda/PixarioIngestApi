namespace Pixario.Ingest.Application.Features.Worker.ImageProcessing;

public class CheckImageStatusDto(
    string status,
    string? nodeId,
    string? nodeType,
    string? exceptionType,
    string? exceptionMessage,
    string? fileName)
{
    public string Status { get; init; } = status;
    public string? NodeId { get; init; } = nodeId;
    public string? NodeType { get; init; } = nodeType;
    public string? ExceptionType { get; init; } = exceptionType;
    public string? ExceptionMessage { get; private set; } = exceptionMessage;
    public string? FileName { get; set; } = fileName;

    public void SetExceptionMessage(string exceptionMessage)
    {
        ExceptionMessage = exceptionMessage;
    }
}