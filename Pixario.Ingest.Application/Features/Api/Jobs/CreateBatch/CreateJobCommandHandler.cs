using Microsoft.Extensions.Logging;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports.Messaging;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Application.Ports.Storage;
using Pixario.Ingest.Core.Domain;
using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Application.Features.Api.Jobs.CreateBatch;

public class CreateJobCommandHandler(
    IFileStorage fileStorage,
    IProcessBatchPublisher publisher,
    IBatchRepository batchRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateJobCommandHandler> log)
{
    public async Task<Guid> Handle(CreateJobCommand request, CancellationToken ct)
    {
        var batchId = Guid.CreateVersion7();
        var batch = RetouchBatch.Create(batchId);
        log.LogInformation("Created batch: {batchId}", batchId);

        foreach (var file in request.Files)
        {
            var imageId = Guid.CreateVersion7();
            var path = await fileStorage.SaveAsync(file.Content, imageId.ToString(), ct);
            log.LogDebug("Saved file: {filePath}", path);

            var image = Image.Create(imageId, file.FileName, imageId, path, file.Size);

            batch.AddImage(image);
            var job = RetouchJob.Create(Guid.CreateVersion7(), batchId, image, JobStatus.Queued);
            log.LogInformation("Created job: {jobId}", job.Id);
            batch.AddJob(job);
            log.LogDebug("Add job {jobId} to batch {batchId}", job.Id, batch.Id);
        }

        await batchRepository.AddAsync(batch, ct);
        await unitOfWork.SaveChangesAsync(ct);

        log.LogDebug("Publishing ProcessBatchMessage for batch {batchId} to RabbitMQ", batch.Id);
        await publisher.PublishAsync(new ProcessBatchMessage
        {
            BatchId = batchId
        }, ct);
        log.LogInformation("ProcessBatchMessage for batch {batchId} published to RabbitMQ", batch.Id);
        return batchId;
    }
}