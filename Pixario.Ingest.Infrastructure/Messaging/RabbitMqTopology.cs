using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;

namespace Pixario.Ingest.Infrastructure.Messaging;

public static class RabbitMqTopology
{
    public static async Task EnsureCreatedAsync(
        IChannel channel,
        RabbitMqOptions opt,
        CancellationToken ct = default)
    {
        await channel.ExchangeDeclareAsync(
            exchange: opt.Exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);
        
        var mainArgs = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = opt.Exchange,
            ["x-dead-letter-routing-key"] = opt.RetryRoutingKey
        };

        await channel.QueueDeclareAsync(
            queue: opt.MainQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: mainArgs,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            queue: opt.MainQueue,
            exchange: opt.Exchange,
            routingKey: opt.RoutingKey,
            arguments: null,
            cancellationToken: ct);
        
        var retryArgs = new Dictionary<string, object?>
        {
            ["x-message-ttl"] = opt.RetryDelayMs,
            ["x-dead-letter-exchange"] = opt.Exchange,
            ["x-dead-letter-routing-key"] = opt.RoutingKey
        };

        await channel.QueueDeclareAsync(
            queue: opt.RetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: retryArgs,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            queue: opt.RetryQueue,
            exchange: opt.Exchange,
            routingKey: opt.RetryRoutingKey,
            arguments: null,
            cancellationToken: ct);
        
        await channel.QueueDeclareAsync(
            queue: opt.DlqQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            queue: opt.DlqQueue,
            exchange: opt.Exchange,
            routingKey: opt.DlqRoutingKey,
            arguments: null,
            cancellationToken: ct);
    }
}