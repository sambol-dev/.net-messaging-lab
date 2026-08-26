namespace Messaging.Shared.Messaging.Topology;

public static class RabbitMqTopology
{
    public const string OrdersExchange = "orders.exchange";
    public const string OrdersQueue = "orders.created";
    public const string OrdersRoutingKey = "orders.created";
    public const string ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
    public const string OrderDeadLetterExchange = "orders.dlx";
    public const string OrderDeadLetterQueue = "orders.created.dlq";
    public const string OrderDeadLetterRoutingKey = "orders.created.dlq";
    public const string RetryExchange = "orders.retry.exchange";
    public const string RetryQueue = "orders.created.retry";
    public const string RetryRoutingKey = "orders.created.retry";
    public const int RetryTtlMiliseconds = 5000;
    public const int MaxRetryAttempts = 3;
    public const string RetryCountReader = "x-retry-count";
}