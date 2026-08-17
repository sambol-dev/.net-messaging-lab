using Messaging.Shared.Messaging.Connection;
using RabbitMQ.Client;

namespace Messaging.Shared.Messaging.Channel;

public sealed class RabbitMqChannelManager : IRabbitMqChannelManager
{
    private readonly IRabbitMqConnectionManager _connectionManager;

    public RabbitMqChannelManager(IRabbitMqConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default)
    {
        var connection = await _connectionManager.GetConnectionAsync(cancellationToken);

        return await connection.CreateChannelAsync(cancellationToken:cancellationToken);
    }
}