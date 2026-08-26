namespace Messaging.Worker.Consumers;

public interface IOrderCreatedConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
}