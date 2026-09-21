namespace Messaging.Worker.Idempotency;

public interface IProcessedMessageStore
{
    Task<bool> ExistsAsync(
        Guid messageId,
        string messageType,
        CancellationToken cancellationToken);
    
    Task MarkAsProcessedAsync(
        Guid messageId,
        string messageType,
        CancellationToken cancellationToken);
}