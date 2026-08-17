using RabbitMQ.Client;

namespace Messaging.Shared.Messaging.Connection;

public interface IRabbitMqConnectionManager : IAsyncDisposable
{
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}