using Pixario.Ingest.Application.Extensions;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports.Integrations;
using Pixario.Ingest.Application.Ports.Messaging;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Application.Ports.Storage;

namespace Pixario.Ingest.Application.Features.Worker.ImageProcessing;

public class CheckImageStatusHandler(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    IProcessBatchPublisher publisher,
    IComfyUiCheckImageStatusGateway gateway,
    IFileStorage storage
)
{
    public async Task<bool> Handle(CheckImageStatusMessage msg, CancellationToken ct)
    {
        var job = await jobRepository.GetAsync(msg.JobId, ct);
        if (job is null) return false;

        job.MarkProcessing();
        await jobRepository.UpdateAsync(job, ct);
        await unitOfWork.SaveChangesAsync(ct);

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

        storage.RenameFile(fileName, job.Image.StoredFileName.ToString());

        job.MarkDone();
        await jobRepository.UpdateAsync(job, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await publisher.PublishAsync(new ProcessBatchMessage
        {
            BatchId = msg.BatchId
        }, ct);

        return true;
    }
}