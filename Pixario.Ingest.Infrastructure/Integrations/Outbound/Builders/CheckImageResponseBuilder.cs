using System.Text.Json;
using Pixario.Ingest.Application.Exceptions;
using Pixario.Ingest.Application.Extensions;
using Pixario.Ingest.Application.Features.Worker.ImageProcessing;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Http.ResponseDto;

namespace Pixario.Ingest.Infrastructure.Integrations.Outbound.Builders;

public class CheckImageResponseBuilder
{
    public CheckImageStatusDto Build(JsonDocument document, string promptId)
    {
        var root = document.RootElement;

        var statusElement = root.GetSection($"{promptId}__status")
                            ?? throw new PermanentProcessingException("Status element not found");

        var status = statusElement.Deserialize<StatusDto>()
                     ?? throw new PermanentProcessingException("Status could not be deserialized");

        if (status.StatusStr == "success")
        {
            var outputsElement = root.GetSection($"{promptId}__outputs")
                                 ?? throw new PermanentProcessingException("Outputs element not found");

            if (outputsElement.ValueKind != JsonValueKind.Object)
                throw new PermanentProcessingException("Outputs element is not an object");

            using var outputsEnumerator = outputsElement.EnumerateObject();

            if (!outputsEnumerator.MoveNext())
                throw new PermanentProcessingException("Outputs element is empty");

            var firstOutput = outputsEnumerator.Current.Value;
            var fileName = firstOutput.GetSection("images__0__filename")?.GetString();

            return new CheckImageStatusDto(
                status.StatusStr,
                null,
                null,
                null,
                null,
                fileName);
        }

        var executionError = status.Messages?
                                 .FirstOrDefault(x => x.Event == "execution_error")
                             ?? throw new PermanentProcessingException("Execution error element not found");

        var errorData = executionError.Data
                        ?? throw new PermanentProcessingException("Execution error data not found");

        return new CheckImageStatusDto(
            status.StatusStr ?? "error",
            errorData.NodeId,
            errorData.NodeType,
            errorData.ExceptionType,
            errorData.ExceptionMessage,
            null);
    }
}