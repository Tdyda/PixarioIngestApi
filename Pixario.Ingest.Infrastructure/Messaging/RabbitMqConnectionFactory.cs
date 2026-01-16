using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Pixario.Ingest.Infrastructure.Messaging;

public interface IRabbitMqConnection : IAsyncDisposable
{
    IConnection Connection { get; }
}

public sealed class RabbitMqConnection : IRabbitMqConnection
{
    public IConnection Connection { get; }

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

    public ValueTask DisposeAsync()
    {
        Connection.Dispose();
        return ValueTask.CompletedTask;
    }
}