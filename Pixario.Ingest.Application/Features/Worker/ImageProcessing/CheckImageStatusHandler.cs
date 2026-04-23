using Microsoft.Extensions.Logging;
using Pixario.Ingest.Application.Exceptions;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports.Integrations;
using Pixario.Ingest.Application.Ports.Messaging;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Application.Ports.Storage;
using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Application.Features.Worker.ImageProcessing;

public class CheckImageStatusHandler(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    IProcessBatchPublisher publisher,
    IComfyUiCheckImageStatusGateway gateway,
    IFileStorage storage,
    ILogger<CheckImageStatusHandler> log,
    CheckImageStatusErrorMapper errorMapper
)
{
    public async Task<bool> Handle(CheckImageStatusMessage msg, CancellationToken ct)
    {
        var job = await jobRepository.GetAsync(msg.JobId, ct);
        if (job is null)
        {
            await PublishNextMessage(msg.BatchId, ct);
            throw new PermanentProcessingException($"Job {msg.JobId} not found");
        }

        if (job.Status != JobStatus.Processing)
        {
            job.MarkProcessing();
            await jobRepository.UpdateAsync(job, ct);
            await unitOfWork.SaveChangesAsync(ct);
            log.LogInformation("Processing started for job {jobId}", msg.JobId);
        }

        var response = await gateway.ProcessAsync(msg.PromptId, ct);
        if (response is null) return false;

        if (response.Status == "success")
        {
            storage.RenameFile(response.FileName!, job.Image.StoredFileName.ToString());

            job.MarkDone();
            await jobRepository.UpdateAsync(job, ct);
            await unitOfWork.SaveChangesAsync(ct);
            log.LogInformation("Processing ended for job {jobId}", msg.JobId);

            await PublishNextMessage(msg.BatchId, ct);

            return true;
        }
        
        errorMapper.Map(response);
        job.JobFailedReason = response.ExceptionMessage!;
        await jobRepository.UpdateAsync(job, ct);
        await unitOfWork.SaveChangesAsync(ct);
        await PublishNextMessage(msg.BatchId, ct);
        throw new PermanentProcessingException(response.ExceptionMessage!);
    }

    private async Task PublishNextMessage(Guid batchId, CancellationToken ct)
    {
        log.LogDebug("Publishing ProcessBatchMessage for batch {batchId} to RabbitMQ", batchId);
        await publisher.PublishAsync(new ProcessBatchMessage
        {
            BatchId = batchId
        }, ct);
        log.LogInformation("ProcessBatchMessage for batch {batchId} published to RabbitMQ", batchId);
    }
}