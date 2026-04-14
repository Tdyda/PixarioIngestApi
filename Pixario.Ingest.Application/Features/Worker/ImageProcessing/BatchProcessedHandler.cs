using Microsoft.Extensions.Logging;
using Pixario.Ingest.Application.Exceptions;
using Pixario.Ingest.Application.Extensions;
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
        if (batch is null)
        {
            throw new PermanentProcessingException("Batch not found");
        }

        try
        {
            return await gateway.ProcessAsync(batch, ct);
        }
        catch (InvalidOperationException ex)
        {
            log.LogError(ex, "Sending to external service failed for batch {batchId}", msg.BatchId);
            throw new PermanentProcessingException(ex.ToString());
        }
        catch(ExternalServiceUnavailableException ex)
        {
            log.LogError(ex, "Sending to external service failed for batch {batchId}", msg.BatchId);
            throw;
        }
        catch (Exception ex)
        {
            batch.MarkFailed();
            await batchRepository.UpdateAsync(batch, ct);
            await unitOfWork.SaveChangesAsync(ct);
            log.LogError(ex, "Processing failed for batch {batchId}", msg.BatchId);
            throw new PermanentProcessingException(ex.ToString());
        }
    }
}