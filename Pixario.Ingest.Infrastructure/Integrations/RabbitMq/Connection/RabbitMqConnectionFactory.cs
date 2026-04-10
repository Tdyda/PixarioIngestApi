using Microsoft.Extensions.Options;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;
using RabbitMQ.Client;

namespace Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Connection;

public interface IRabbitMqConnection : IAsyncDisposable
{
    IConnection Connection { get; }
}

public sealed class RabbitMqConnection : IRabbitMqConnection
{
    public RabbitMqConnection(IOptions<RabbitMqOptions> options)
    {
        var opt = options.Value;

        var factory = new ConnectionFactory
        {
            HostName = opt.Host,
            Port = opt.Port,
            UserName = opt.User,
            Password = opt.Password,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true
        };

        Connection = factory
            .CreateConnectionAsync("pixario.ingest")
            .GetAwaiter()
            .GetResult();
    }

    public IConnection Connection { get; }

    public ValueTask DisposeAsync()
    {
        Connection.Dispose();
        return ValueTask.CompletedTask;
    }
}