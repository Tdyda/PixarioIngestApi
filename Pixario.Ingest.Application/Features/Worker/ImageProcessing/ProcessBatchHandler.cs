using Microsoft.Extensions.Logging;
using Pixario.Ingest.Application.Exceptions;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports.Integrations;
using Pixario.Ingest.Application.Ports.Messaging;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Application.Features.Worker.ImageProcessing;

public class ProcessBatchHandler(
    IImageProcessingGateway gateway,
    IBatchRepository batchRepository,
    IUnitOfWork unitOfWork,
    ICheckImageProcessingStatusPublisher publisher,
    IBatchProcessedNotifier notifier,
    ILogger<ProcessBatchHandler> log)
{
    public async Task Handle(ProcessBatchMessage msg, CancellationToken ct)
    {
        var batch = await batchRepository.GetAsync(msg.BatchId, ct);

        if (batch is null)
            throw new PermanentProcessingException($"Batch {msg.BatchId} not found");

        if (batch.Status == JobStatus.Queued)
        {
            batch.MarkProcessing();
            await batchRepository.UpdateAsync(batch, ct);
            await unitOfWork.SaveChangesAsync(ct);
            log.LogInformation("Processing started for batch {batchId}", msg.BatchId);
        }

        var nextJob = batch.GetNextPendingJob();

        if (nextJob is null)
        {
            batch.MarkDone();
            await batchRepository.UpdateAsync(batch, ct);
            await unitOfWork.SaveChangesAsync(ct);
            log.LogInformation("Processing ended for batch {batchId}", msg.BatchId);

            log.LogDebug("Publishing BatchProcessedMessage for batch {batchId} to RabbitMQ", msg.BatchId);
            await notifier.PublishAsync(new BatchProcessedMessage
            (
                msg.BatchId
            ), ct);
            log.LogInformation("BatchProcessedMessage for batch {batchId} published to RabbitMQ", msg.BatchId);

            return;
        }

        var promptId = await gateway.ProcessAsync(nextJob.Image.StoredFileName.ToString(), ct);
        log.LogDebug("PromptId {promptId}", promptId);

        log.LogDebug("Publishing CheckImageStatusMessage for job {jobId} to RabbitMQ", nextJob.Id);
        await publisher.PublishAsync(
            new CheckImageStatusMessage(
                msg.BatchId,
                nextJob.Id,
                promptId,
                DateTime.UtcNow
            ), ct);
        log.LogInformation("CheckImageStatusMessage for job {jobId} published to RabbitMQ", nextJob.Id);
    }
}