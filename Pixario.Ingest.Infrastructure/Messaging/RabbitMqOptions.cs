namespace Pixario.Ingest.Infrastructure.Messaging;

public class RabbitMqOptions
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string User { get; init; } = "guest";
    public string Password { get; init; } = "guest";

    public string Exchange { get; init; } = "pixario.ingest";

    public string MainQueue { get; init; } = "pixario.ingest.images";
    public string RetryQueue { get; init; } = "pixario.ingest.images.retry";
    public string DlqQueue { get; init; } = "pixario.ingest.images.dlq";

    public string RoutingKey { get; init; } = "images.process.requested";
    public string RetryRoutingKey { get; init; } = "images.process.retry";
    public string DlqRoutingKey { get; init; } = "images.process.dlq";

    public int RetryDelayMs { get; init; } = 30000;
}
