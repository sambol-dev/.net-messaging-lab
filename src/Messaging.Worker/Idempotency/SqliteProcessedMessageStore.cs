using Messaging.Worker.Data;
using Microsoft.EntityFrameworkCore;

namespace Messaging.Worker.Idempotency;

public class SqliteProcessedMessageStore : IProcessedMessageStore
{
    private readonly MessagingDbContext _dbContext;

    public SqliteProcessedMessageStore(MessagingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(
        Guid messageId,
        string messageType,
        CancellationToken cancellationToken)
    {
        return _dbContext.ProcessedMessages
            .AnyAsync(
                x => x.MessageId == messageId &&
                x.MessageType == messageType,
                cancellationToken);
    }

    public async Task MarkAsProcessedAsync(
        Guid messageId,
        string messageType,
        CancellationToken cancellationToken)
    {
        var message = new ProcessedMessage
        {
            MessageId = messageId,
            MessageType = messageType,
            ProcessedAt = DateTime.UtcNow
        };

        _dbContext.ProcessedMessages.Add(message);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}