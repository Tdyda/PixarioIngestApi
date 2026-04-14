using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports.Integrations;
using Pixario.Ingest.Application.Ports.Messaging;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Application.Features.Worker.ImageProcessing;

public class ProcessBatchHandler(
    IImageProcessingGateway gateway,
    IJobRepository jobRepository,
    IBatchRepository batchRepository,
    IUnitOfWork unitOfWork,
    ICheckImageProcessingStatusPublisher publisher,
    IBatchProcessedNotifier notifier)
{
    public async Task Handle(ProcessBatchMessage msg, CancellationToken ct)
    {
        var batch = await batchRepository.GetAsync(msg.BatchId, ct);

        if (batch is null)
            throw new InvalidOperationException($"Batch {msg.BatchId} not found");

        if (batch.Status == JobStatus.Queued)
        {
            batch.MarkProcessing();
            await batchRepository.UpdateAsync(batch, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }

        var nextJob = batch.GetNextPendingJob();

        if (nextJob is null)
        {
            batch.MarkDone();
            await batchRepository.UpdateAsync(batch, ct);
            await unitOfWork.SaveChangesAsync(ct);

            await notifier.PublishAsync(new BatchProcessedMessage
            {
                BatchId = msg.BatchId
            }, ct);

            return;
        }

        try
        {
            var promptId = await gateway.ProcessAsync(nextJob.Image.FileName, ct);

            await publisher.PublishAsync(
                new CheckImageStatusMessage
                {
                    BatchId = msg.BatchId,
                    JobId = nextJob.Id,
                    PromptId = promptId,
                    CompletedAt = DateTime.UtcNow
                }, ct);
        }
        catch (Exception ex)
        {
            nextJob.MarkFailed();
            await jobRepository.UpdateAsync(nextJob, ct);
            await unitOfWork.SaveChangesAsync(ct);

            throw new Exception(
                $"Pipeline failed, {ex.Message}",
                ex);
        }
    }
}