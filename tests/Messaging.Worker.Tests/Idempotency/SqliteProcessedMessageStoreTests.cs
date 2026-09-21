using Messaging.Worker.Data;
using Messaging.Worker.Idempotency;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Messaging.Worker.Tests.Idempotency;

public class SqliteProcessedMessageStoreTests
{
    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenMessageDoesNotExist()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext =
            new MessagingDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        var store =
            new SqliteProcessedMessageStore(dbContext);

        var messageId = Guid.NewGuid();

        var exists = await store.ExistsAsync(
            messageId,
            "OrderCreated",
            CancellationToken.None);

        Assert.False(exists);
    }

    [Fact]
    public async Task MarkAsProcessedAsync_ShouldPersistMessage()
    {
        var databasePath = CreateTemporaryDatabasePath();

        try
        {
            var options = CreateDbContextOptions(databasePath);

            await using var dbContext =
                new MessagingDbContext(options);

            await dbContext.Database.EnsureCreatedAsync();

            var store =
                new SqliteProcessedMessageStore(dbContext);

            var messageId = Guid.NewGuid();

            await store.MarkAsProcessedAsync(
                messageId,
                "OrderCreated",
                CancellationToken.None);

            var exists = await store.ExistsAsync(
                messageId,
                "OrderCreated",
                CancellationToken.None);

            Assert.True(exists);
        }
        finally
        {
            DeleteDatabase(databasePath);
        }
    }

    [Fact]
    public async Task ProcessedMessage_ShouldRemainAvailable_WithNewDbContext()
    {
        var databasePath = CreateTemporaryDatabasePath();

        try
        {
            var options = CreateDbContextOptions(databasePath);

            var messageId = Guid.NewGuid();

            await using (var firstContext =
                new MessagingDbContext(options))
            {
                await firstContext.Database.EnsureCreatedAsync();

                var store =
                    new SqliteProcessedMessageStore(firstContext);

                await store.MarkAsProcessedAsync(
                    messageId,
                    "OrderCreated",
                    CancellationToken.None);
            }

            await using (var secondContext =
                new MessagingDbContext(options))
            {
                var store =
                    new SqliteProcessedMessageStore(secondContext);

                var exists = await store.ExistsAsync(
                    messageId,
                    "OrderCreated",
                    CancellationToken.None);

                Assert.True(exists);
            }
        }
        finally
        {
            DeleteDatabase(databasePath);
        }
    }

    [Fact]
    public async Task MarkAsProcessedAsync_ShouldRejectDuplicateMessage()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseSqlite(connection)
            .Options;

        var messageId = Guid.NewGuid();

        await using (var firstContext =
            new MessagingDbContext(options))
        {
            await firstContext.Database.EnsureCreatedAsync();

            var store =
                new SqliteProcessedMessageStore(firstContext);

            await store.MarkAsProcessedAsync(
                messageId,
                "OrderCreated",
                CancellationToken.None);
        }

        await using (var secondContext =
            new MessagingDbContext(options))
        {
            var store =
                new SqliteProcessedMessageStore(secondContext);

            await Assert.ThrowsAsync<DbUpdateException>(() =>
                store.MarkAsProcessedAsync(
                    messageId,
                    "OrderCreated",
                    CancellationToken.None));
        }
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenMessageTypeIsDifferent()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext =
            new MessagingDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        var store =
            new SqliteProcessedMessageStore(dbContext);

        var messageId = Guid.NewGuid();

        await store.MarkAsProcessedAsync(
            messageId,
            "OrderCreated",
            CancellationToken.None);

        var exists = await store.ExistsAsync(
            messageId,
            "AnotherMessage",
            CancellationToken.None);

        Assert.False(exists);
    }

    private static DbContextOptions<MessagingDbContext>
        CreateDbContextOptions(string databasePath)
    {
        return new DbContextOptionsBuilder<MessagingDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;
    }

    private static string CreateTemporaryDatabasePath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            $"messaging-worker-tests-{Guid.NewGuid():N}.db");
    }

    private static void DeleteDatabase(string databasePath)
    {
        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }

        var walPath = $"{databasePath}-wal";
        var shmPath = $"{databasePath}-shm";

        if (File.Exists(walPath))
        {
            File.Delete(walPath);
        }

        if (File.Exists(shmPath))
        {
            File.Delete(shmPath);
        }
    }
}