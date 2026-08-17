namespace Messaging.Api.Messaging.Topology;

public static class RabbitMqTopology
{
    public const string OrdersExchange = "orders.exchange";
    public const string OrdersQueue = "orders.created";
    public const string OrdersRoutingKey = "orders.created";
    public const string ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
}