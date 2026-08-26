namespace Messaging.Worker.Messaging.Retry;

public interface IRabbitMqRetryPublisher
{
    Task PublishAsync(
        byte[] body,
        int retryCount,
        CancellationToken cancellationToken);
}