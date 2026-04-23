using Pixario.Ingest.Core.Domain;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Builders;

namespace Pixario.Ingest.Infrastructure.Integrations.Outbound.Extensions;

public static class PixarioBatchProcessedPayloadBuilderExtensions
{
    extension(Task<PixarioBatchProcessedPayloadBuilder> task)
    {
        public async Task<PixarioBatchProcessedPayloadBuilder> AddResultsMapAsync(RetouchBatch batch)
        {
            var builder = await task;
            return await builder.AddResultsMapAsync(batch);
        }

        public async Task<HttpRequestMessage> BuildAsync()
        {
            var builder = await task;
            return await builder.BuildAsync();
        }
    }
}