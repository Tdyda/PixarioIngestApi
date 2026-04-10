namespace Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;

public class RabbitMqOptions
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string User { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string Exchange { get; init; } = "pixario.ingest";

    public string BatchProcessQueue { get; init; } = "batch.process";
    public string BatchProcessRetryQueue { get; init; } = "batch.process.retry";
    public string BatchProcessDlqQueue { get; init; } = "batch.process.dlq";
    public string ImageStatusCheckQueue { get; set; } = "image.status.check";
    public string ImageStatusCheckRetryQueue { get; set; } = "image.status.check.retry";
    public string ImageStatusCheckDlqQueue { get; set; } = "image.status.check.dlq";
    public string BatchProcessedNotifyQueue { get; set; } = "batch.processed.notify";
    public string BatchProcessedNotifyRetryQueue { get; set; } = "batch.processed.notify.retry";
    public string BatchProcessedNotifyDlqQueue { get; set; } = "batch.processed.notify.dlq";

    public string BatchProcessRoutingKey { get; init; } = "batch.process.requested";
    public string BatchProcessRetryRoutingKey { get; init; } = "batch.process.retry";
    public string BatchProcessDlqRoutingKey { get; init; } = "batch.process.dlq";
    public string ImageStatusCheckRoutingKey { get; set; } = "image.status.check.requested";
    public string ImageStatusCheckRetryRoutingKey { get; set; } = "image.status.check.retry";
    public string ImageStatusCheckDlqRoutingKey { get; set; } = "image.status.check.dlq";
    public string BatchProcessedNotifyRoutingKey { get; set; } = "batch.processed.notify.requested";
    public string BatchProcessedNotifyRetryRoutingKey { get; set; } = "batch.processed.retry";
    public string BatchProcessedNotifyDlqRoutingKey { get; set; } = "batch.processed.notify.dlq";


    public int RetryDelayMs { get; init; } = 30000;
    public int CheckImageStatusRetryDelayMs { get; init; } = 10000;
}