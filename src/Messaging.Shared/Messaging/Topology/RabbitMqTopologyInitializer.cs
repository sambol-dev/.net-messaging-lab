using Messaging.Shared.Messaging.Channel;
using Microsoft.Extensions.Logging;

namespace Messaging.Shared.Messaging.Topology;

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

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqTopology.OrderDeadLetterExchange,
            type: RabbitMqTopology.ExchangeType,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "Dead Letter Exchange '{Exchange}' criada/verificada",
            RabbitMqTopology.OrderDeadLetterExchange
        );

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqTopology.RetryExchange,
            type: RabbitMqTopology.ExchangeType,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "Retry Exchange '{Exchange}' criada/verificada",
            RabbitMqTopology.RetryExchange
        );

        var queueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = RabbitMqTopology.OrderDeadLetterExchange,
            ["x-dead-letter-routing-key"] = RabbitMqTopology.OrderDeadLetterRoutingKey
        };

        await channel.QueueDeclareAsync(
            queue: RabbitMqTopology.OrdersQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "Queue '{Queue}' criada/verificada'",
            RabbitMqTopology.OrdersQueue
        );

        var retryArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = RabbitMqTopology.OrdersExchange,
            ["x-dead-letter-routing-key"] = RabbitMqTopology.OrdersRoutingKey
        };

        await channel.QueueDeclareAsync(
            queue: RabbitMqTopology.RetryQueue,
            durable: true,
            exclusive: false,
            autoDelete:false,
            arguments: retryArguments,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Retry Queue '{Queue}' criada/verificada",
            RabbitMqTopology.RetryQueue);

        await channel.QueueDeclareAsync(
            queue: RabbitMqTopology.OrderDeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "Dead Letter Queue '{Queue}' criada/verificada",
            RabbitMqTopology.OrderDeadLetterQueue
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

        await channel.QueueBindAsync(
            queue: RabbitMqTopology.RetryQueue,
            exchange: RabbitMqTopology.RetryExchange,
            routingKey: RabbitMqTopology.RetryRoutingKey,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Binding criado entre Retry Exchange '{Exchange}' " +
            "e Retry Queue '{Queue}'",
            RabbitMqTopology.RetryExchange,
            RabbitMqTopology.RetryQueue);

        await channel.QueueBindAsync(
            queue: RabbitMqTopology.OrderDeadLetterQueue,
            exchange: RabbitMqTopology.OrderDeadLetterExchange,
            routingKey: RabbitMqTopology.OrderDeadLetterRoutingKey,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "Binding criado entre Dead Letter Exchange '{Exchange}' " +
            "e Dead LetterQueue '{Queue}'",
            RabbitMqTopology.OrderDeadLetterExchange,
            RabbitMqTopology.OrderDeadLetterQueue
        );

        _logger.LogInformation(
            "Topologia RabbitMQ inicializada com sucesso.");
    }
}