using Messaging.Shared.Messaging.Channel;
using Messaging.Shared.Messaging.Topology;
using RabbitMQ.Client;

namespace Messaging.Worker.Messaging.Retry;

public class RabbitMqRetryPublisher : IRabbitMqRetryPublisher
{
    private readonly IRabbitMqChannelManager _channelManager;

    public RabbitMqRetryPublisher(IRabbitMqChannelManager channelManager)
    {
        _channelManager = channelManager;
    }

    public async Task PublishAsync(
        byte[] body,
        int retryCount,
        CancellationToken cancellationToken
    )
    {
        await using var channel = await _channelManager.CreateChannelAsync(cancellationToken);

        var headers = new Dictionary<string, object?>
        {
            [RabbitMqTopology.RetryCountReader] = retryCount
        };

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Headers = headers
        };

        await channel.BasicPublishAsync(
            exchange: RabbitMqTopology.RetryExchange,
            routingKey: RabbitMqTopology.RetryRoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}