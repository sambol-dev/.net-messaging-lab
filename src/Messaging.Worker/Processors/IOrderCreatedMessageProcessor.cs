namespace Messaging.Worker.Processors;

public interface IOrderCreatedMessageProcessor
{
    Task ProcessAsync(
        byte[] body,
        CancellationToken cancellationToken);
}