using RabbitMQ.Client;

namespace ms_notification.Services;

public interface IRabbitMQConnection : IAsyncDisposable
{
    Task<IChannel> CreateChannelAsync();
}