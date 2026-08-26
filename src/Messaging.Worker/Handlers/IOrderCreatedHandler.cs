using Messaging.Shared.Contracts;

namespace Messaging.Worker.Handlers;

public interface IOrderCreatedHandler
{
    Task HandleAsync(
        OrderCreated message,
        CancellationToken cancellationToken
    );
}