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
    IUnitOfWork unitOfWork)
{
    public async Task<Guid> Handle(CreateJobCommand request, CancellationToken ct)
    {
        var batchId = Guid.CreateVersion7();
        var batch = RetouchBatch.Create(batchId);

        foreach (var file in request.Files)
        {
            var path = await fileStorage.SaveAsync(file.Content, file.FileName, ct);

            var imageId = Guid.CreateVersion7();
            var image = Image.Create(imageId, file.FileName, path, file.Size);

            batch.AddImage(image);

            var job = RetouchJob.Create(Guid.CreateVersion7(), batchId, image, JobStatus.Queued);
            batch.AddJob(job);
        }

        await batchRepository.AddAsync(batch, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await publisher.PublishAsync(new ProcessBatchMessage
        {
            BatchId = batchId
        }, ct);
        return batchId;
    }
}