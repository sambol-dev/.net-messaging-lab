namespace Messaging.Shared.Contracts;

public sealed record OrderCreated(
    Guid OrderId,
    DateTime CreatedAt,
    decimal Total);