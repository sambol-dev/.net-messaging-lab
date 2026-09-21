using System.Text.Json;
using Messaging.Shared.Contracts;
using Messaging.Worker.Data;
using Messaging.Worker.Exceptions;
using Messaging.Worker.Handlers;
using Messaging.Worker.Idempotency;
using Messaging.Worker.Processors;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Messaging.Worker.Tests.Processors;

public class OrderCreatedMessageProcessorTests
{
    [Fact]
    public async Task ProcessAsync_ShouldNotCallHandler_WhenMessageWasAlreadyProcessed()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<MessagingDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var dbContext =
            new MessagingDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        var store =
            new SqliteProcessedMessageStore(dbContext);

        var handler =
            new TestOrderCreatedHandler();

        var order = new OrderCreated(
            Guid.NewGuid(),
            DateTime.UtcNow,
            100);

        await store.MarkAsProcessedAsync(
            order.OrderId,
            "OrderCreated",
            CancellationToken.None);

        var processor =
            new OrderCreatedMessageProcessor(
                handler,
                store,
                NullLogger<OrderCreatedMessageProcessor>.Instance);

        var body =
            JsonSerializer.SerializeToUtf8Bytes(order);

        await processor.ProcessAsync(
            body,
            CancellationToken.None);

        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task ProcessAsync_ShouldProcessAndMarkMessage_WhenMessageIsNew()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<MessagingDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var dbContext =
            new MessagingDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        var store =
            new SqliteProcessedMessageStore(dbContext);

        var handler =
            new TestOrderCreatedHandler();

        var order = new OrderCreated(
            Guid.NewGuid(),
            DateTime.UtcNow,
            100);

        var processor =
            new OrderCreatedMessageProcessor(
                handler,
                store,
                NullLogger<OrderCreatedMessageProcessor>.Instance);

        var body =
            JsonSerializer.SerializeToUtf8Bytes(order);

        await processor.ProcessAsync(
            body,
            CancellationToken.None);

        Assert.Equal(1, handler.CallCount);

        var processed =
            await store.ExistsAsync(
                order.OrderId,
                "OrderCreated",
                CancellationToken.None);

        Assert.True(processed);
    }

    [Fact]
    public async Task ProcessAsync_ShouldNotMarkMessage_WhenHandlerFails()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<MessagingDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var dbContext =
            new MessagingDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        var store =
            new SqliteProcessedMessageStore(dbContext);

        var handler =
            new FailingOrderCreatedHandler();

        var order = new OrderCreated(
            Guid.NewGuid(),
            DateTime.UtcNow,
            100);

        var processor =
            new OrderCreatedMessageProcessor(
                handler,
                store,
                NullLogger<OrderCreatedMessageProcessor>.Instance);

        var body =
            JsonSerializer.SerializeToUtf8Bytes(order);

        await Assert.ThrowsAsync<TransientException>(() =>
            processor.ProcessAsync(
                body,
                CancellationToken.None));

        var processed =
            await store.ExistsAsync(
                order.OrderId,
                "OrderCreated",
                CancellationToken.None);

        Assert.False(processed);
    }

    private sealed class FailingOrderCreatedHandler
    : IOrderCreatedHandler
    {
        public Task HandleAsync(
            OrderCreated message,
            CancellationToken cancellationToken)
        {
            throw new TransientException(
                "Falha transitória simulada.");
        }
    }
    private sealed class TestOrderCreatedHandler
        : IOrderCreatedHandler
    {
        public int CallCount { get; private set; }

        public Task HandleAsync(
            OrderCreated message,
            CancellationToken cancellationToken)
        {
            CallCount++;

            return Task.CompletedTask;
        }
    }
}