using RabbitMQ.Client;

namespace Messaging.Shared.Messaging.Channel;

public interface IRabbitMqChannelManager
{
    Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default);
}