using Pixario.Ingest.Application.Extensions;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports.Integrations;
using Pixario.Ingest.Application.Ports.Messaging;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Application.Ports.Storage;

namespace Pixario.Ingest.Application.Features.Worker.ImageProcessing;

public class CheckImageStatusHandler(
    IJobRepository jobRepository,
    IProcessBatchPublisher publisher,
    IComfyUiCheckImageStatusGateway gateway,
    IFileStorage storage
)
{
    public async Task<bool> Handle(CheckImageStatusMessage msg, CancellationToken ct)
    {
        msg.Job.MarkProcessing();
        await jobRepository.UpdateAsync(msg.Job, ct);

        var historyDoc = await gateway.ProcessAsync(msg.PromptId, ct);
        if (historyDoc is null) return false;

        var root = historyDoc!.RootElement;

        var outputs = root.GetSection($"{msg.PromptId}__outputs");

        var firstOutput = outputs?.EnumerateObject().First().Value;

        var fileName = firstOutput?
            .GetSection("images__0__filename")
            ?.GetString();

        if (fileName is null)
            //throw permanent failure
            return false;

        storage.RenameFile(fileName, msg.Job.Image.FileName);

        msg.Job.MarkDone();
        await jobRepository.UpdateAsync(msg.Job, ct);

        await publisher.PublishAsync(new ProcessBatchMessage
        {
            BatchId = msg.BatchId
        }, ct);

        return true;
    }
}