using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports.Integrations;
using Pixario.Ingest.Application.Ports.Repositories;

namespace Pixario.Ingest.Application.Features.Worker.ImageProcessing;

public class BatchProcessedDlqHandler(
    IBatchRepository batchRepository,
    IPixarioBatchProcessedGateway gateway)
{
    public async Task<HttpResponseMessage> Handle(BatchProcessedMessage msg, CancellationToken ct)
    {
        var batch = await batchRepository.GetAsync(msg.BatchId, ct);

        if (batch is null)
            throw new Exception("Unexpected problem, batch shouldn't be null at this point.");
        
        return await gateway.ProcessDlqAsync(batch, ct);
    }
}