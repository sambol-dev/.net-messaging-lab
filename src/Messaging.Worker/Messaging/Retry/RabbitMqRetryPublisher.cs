using Messaging.Shared.Messaging.Channel;
using Messaging.Shared.Messaging.Topology;
using Messaging.Worker.Exceptions;
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
        TimeSpan retryDelay,
        CancellationToken cancellationToken
    )
    {
        await using var channel = await _channelManager.CreateChannelAsync(cancellationToken);

        var headers = new Dictionary<string, object?>
        {
            [RabbitMqTopology.RetryCountReader] = retryCount
        };

        var expiration = ((long)retryDelay.TotalMilliseconds).ToString();
        
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Expiration = expiration,
            Headers = headers
        };

        try
        {
            await channel.BasicPublishAsync(
                exchange: RabbitMqTopology.RetryExchange,
                routingKey: RabbitMqTopology.RetryRoutingKey,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);    
        }
        catch (Exception ex)
        {
            throw new RetryPublishException(
                "Falha ao publicar mensagem na fila de retry",
            ex);
        }
        
    }
}