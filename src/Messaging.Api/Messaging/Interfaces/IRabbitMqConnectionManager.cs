using RabbitMQ.Client;

namespace Messaging.Api.Messaging.Interfaces;

public interface IRabbitMqConnectionManager : IAsyncDisposable
{
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}