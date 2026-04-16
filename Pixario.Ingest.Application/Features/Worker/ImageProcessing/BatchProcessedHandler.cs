using Microsoft.Extensions.Logging;
using Pixario.Ingest.Application.Exceptions;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports.Integrations;
using Pixario.Ingest.Application.Ports.Repositories;

namespace Pixario.Ingest.Application.Features.Worker.ImageProcessing;

public class BatchProcessedHandler(
    IBatchRepository batchRepository,
    IUnitOfWork unitOfWork,
    IPixarioBatchProcessedGateway gateway,
    ILogger<BatchProcessedHandler> log
)
{
    public async Task<HttpResponseMessage> Handle(BatchProcessedMessage msg, CancellationToken ct)
    {
        var batch = await batchRepository.GetAsync(msg.BatchId, ct);
        if (batch is null) throw new PermanentProcessingException("Batch not found");

        try
        {
            return await gateway.ProcessAsync(batch, ct);
        }
        catch (Exception)
        {
            batch.MarkFailed();
            await batchRepository.UpdateAsync(batch, ct);
            await unitOfWork.SaveChangesAsync(ct);
            throw;
        }
    }
}