using Microsoft.Extensions.Options;
using ms_notification.Options;
using RabbitMQ.Client;

namespace ms_notification.Services;


public sealed class RabbitMQConnection : IRabbitMQConnection
{
    private IConnection? _connection;
    private readonly RabbitMQSettings _settings;
    private bool _disposed;

    public RabbitMQConnection( IOptions<RabbitMQSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<IChannel> CreateChannelAsync()
    {
        if (_connection is null || !_connection.IsOpen)
            await ConnectAsync();

        return await _connection!.CreateChannelAsync();
    }

    private async Task ConnectAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port     = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password
        };

        _connection = await factory.CreateConnectionAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}