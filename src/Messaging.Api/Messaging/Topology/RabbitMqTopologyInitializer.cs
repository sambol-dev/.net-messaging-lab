using Messaging.Api.Messaging.Channel;

namespace Messaging.Api.Messaging.Topology;

public class RabbitMqTopologyInitializer : IRabbitMqTopologyInitializer
{
    private readonly IRabbitMqChannelManager _channelManager;
    private readonly ILogger<RabbitMqTopologyInitializer> _logger;

    public RabbitMqTopologyInitializer(
        IRabbitMqChannelManager channelManager,
        ILogger<RabbitMqTopologyInitializer> logger)
    {
        _channelManager = channelManager;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelManager.CreateChannelAsync(cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqTopology.OrdersExchange,
            type: RabbitMqTopology.ExchangeType,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "Exchange '{Exchange}' criada/verificada",
            RabbitMqTopology.OrdersExchange
        );

        await channel.QueueDeclareAsync(
            queue: RabbitMqTopology.OrdersQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "Queue '{Queue} criada/verificada'",
            RabbitMqTopology.OrdersQueue
        );

        await channel.QueueBindAsync(
            queue: RabbitMqTopology.OrdersQueue,
            exchange: RabbitMqTopology.OrdersExchange,
            routingKey: RabbitMqTopology.OrdersRoutingKey,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "Binding criado entre '{Exchange}' e '{Queue}'",
            RabbitMqTopology.OrdersExchange,
            RabbitMqTopology.OrdersQueue
        );

        _logger.LogInformation(
            "Topologia RabbitMQ inicializada com sucesso.");
    }
}