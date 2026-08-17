using System.Text.Json;
using Messaging.Api.Messaging.Topology;
using Messaging.Shared.Messaging.Channel;
using RabbitMQ.Client;

namespace Messaging.Api.Messaging.Publisher;

public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly IRabbitMqChannelManager _channelManager;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(
        IRabbitMqChannelManager channelManager,
        ILogger<RabbitMqPublisher> logger)
    {
        _channelManager = channelManager;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, string exchange, string routingkey, CancellationToken cancellationToken = default)
    {
        await using var channel = 
            await _channelManager.CreateChannelAsync(cancellationToken);
        
        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        var basicProperties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"    
        };

        await channel.BasicPublishAsync(
            exchange: RabbitMqTopology.OrdersExchange,
            routingKey: RabbitMqTopology.OrdersRoutingKey,
            mandatory: false,
            basicProperties: basicProperties,
            body: body,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "Mensagem publicada na Exchange '{Exchange}' com Routing Key '{RoutingKey}'.",
            RabbitMqTopology.OrdersExchange,
            RabbitMqTopology.OrdersRoutingKey);
    }
}