using RabbitMQ.Client;

namespace Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;

public static class RabbitMqTopology
{
    public static async Task EnsureCreatedAsync(
        IChannel channel,
        RabbitMqOptions opt,
        CancellationToken ct = default)
    {
        await channel.ExchangeDeclareAsync(
            opt.Exchange,
            ExchangeType.Direct,
            true,
            false,
            cancellationToken: ct);

        #region BatchProcessQueue

        var batchProcessArgs = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = opt.Exchange,
            ["x-dead-letter-routing-key"] = opt.BatchProcessRetryRoutingKey
        };

        await channel.QueueDeclareAsync(
            opt.BatchProcessQueue,
            true,
            false,
            false,
            batchProcessArgs,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            opt.BatchProcessQueue,
            opt.Exchange,
            opt.BatchProcessRoutingKey,
            cancellationToken: ct);

        #endregion

        #region BatchProcessRetryQueue

        var batchProcessRetryArgs = new Dictionary<string, object?>
        {
            ["x-message-ttl"] = opt.RetryDelayMs,
            ["x-dead-letter-exchange"] = opt.Exchange,
            ["x-dead-letter-routing-key"] = opt.BatchProcessRoutingKey
        };

        await channel.QueueDeclareAsync(
            opt.BatchProcessRetryQueue,
            true,
            false,
            false,
            batchProcessRetryArgs,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            opt.BatchProcessRetryQueue,
            opt.Exchange,
            opt.BatchProcessRetryRoutingKey,
            cancellationToken: ct);

        #endregion

        #region BatchProcessDlqQueue

        await channel.QueueDeclareAsync(
            opt.BatchProcessDlqQueue,
            true,
            false,
            false,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            opt.BatchProcessDlqQueue,
            opt.Exchange,
            opt.BatchProcessDlqRoutingKey,
            cancellationToken: ct);

        #endregion

        #region ImageStatusCheckQueue

        var imageStatusArgs = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = opt.Exchange,
            ["x-dead-letter-routing-key"] = opt.ImageStatusCheckRetryRoutingKey
        };

        await channel.QueueDeclareAsync(
            opt.ImageStatusCheckQueue,
            true,
            false,
            false,
            imageStatusArgs,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            opt.ImageStatusCheckQueue,
            opt.Exchange,
            opt.ImageStatusCheckRoutingKey,
            cancellationToken: ct);

        #endregion

        #region ImageStatusCheckRetryQueue

        var imageStatusCheckRetryArgs = new Dictionary<string, object?>
        {
            ["x-message-ttl"] = opt.CheckImageStatusRetryDelayMs,
            ["x-dead-letter-exchange"] = opt.Exchange,
            ["x-dead-letter-routing-key"] = opt.ImageStatusCheckRoutingKey
        };
        await channel.QueueDeclareAsync(
            opt.ImageStatusCheckRetryQueue,
            true,
            false,
            false,
            imageStatusCheckRetryArgs,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            opt.ImageStatusCheckRetryQueue,
            opt.Exchange,
            opt.ImageStatusCheckRetryRoutingKey,
            cancellationToken: ct);

        #endregion

        #region ImageStatusCheckDlqQueue

        await channel.QueueDeclareAsync(
            opt.ImageStatusCheckDlqQueue,
            true,
            false,
            false,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            opt.ImageStatusCheckDlqQueue,
            opt.Exchange,
            opt.ImageStatusCheckDlqRoutingKey,
            cancellationToken: ct);

        #endregion

        #region BatchProcessedNotifyQueue

        var batchProcessedNotifyArgs = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = opt.Exchange,
            ["x-dead-letter-routing-key"] = opt.BatchProcessedNotifyRetryRoutingKey
        };

        await channel.QueueDeclareAsync(
            opt.BatchProcessedNotifyQueue,
            true,
            false,
            false,
            batchProcessedNotifyArgs,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            opt.BatchProcessedNotifyQueue,
            opt.Exchange,
            opt.BatchProcessedNotifyRoutingKey,
            cancellationToken: ct);

        #endregion

        #region BatchProcessedNotifyRetryQueue

        var batchProcessedNotifyRetry = new Dictionary<string, object?>
        {
            ["x-message-ttl"] = opt.RetryDelayMs,
            ["x-dead-letter-exchange"] = opt.Exchange,
            ["x-dead-letter-routing-key"] = opt.BatchProcessedNotifyRoutingKey
        };

        await channel.QueueDeclareAsync(
            opt.BatchProcessedNotifyRetryQueue,
            true,
            false,
            false,
            batchProcessedNotifyRetry,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            opt.BatchProcessedNotifyRetryQueue,
            opt.Exchange,
            opt.BatchProcessedNotifyRetryRoutingKey,
            cancellationToken: ct);

        #endregion

        #region BatchProcessedNotifyDlqQueue

        await channel.QueueDeclareAsync(
            opt.BatchProcessedNotifyDlqQueue,
            true,
            false,
            false,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            opt.BatchProcessedNotifyDlqQueue,
            opt.Exchange,
            opt.BatchProcessedNotifyDlqRoutingKey,
            cancellationToken: ct);

        #endregion
    }
}