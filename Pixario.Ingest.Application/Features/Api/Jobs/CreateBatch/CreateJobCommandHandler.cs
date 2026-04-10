using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports.Messaging;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Application.Ports.Storage;
using Pixario.Ingest.Core.Entities;

namespace Pixario.Ingest.Application.Features.Api.Jobs.CreateBatch;

public class CreateJobCommandHandler(
    IFileStorage fileStorage,
    IProcessBatchPublisher publisher,
    IBatchRepository batchRepository)
{
    public async Task<Guid> Handle(CreateJobCommand request, CancellationToken ct)
    {
        var batchId = Guid.NewGuid();
        var batch = new ImageRetouchBatch(batchId);

        foreach (var file in request.Files)
        {
            var path = await fileStorage.SaveAsync(file.Content, file.FileName, ct);

            var image = new ImageAsset(Guid.NewGuid(), file.FileName, path, file.Size);

            batch.AddImage(image);

            var job = new ImageRetouchJob(batchId, image);
            batch.AddJob(job);
        }

        await batchRepository.AddAsync(batch, ct);
        await batchRepository.SaveAsync(ct);

        await publisher.PublishAsync(new ProcessBatchMessage
        {
            BatchId = batchId
        }, ct);
        return batchId;
    }
}