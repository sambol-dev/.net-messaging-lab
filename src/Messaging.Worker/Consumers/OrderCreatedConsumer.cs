using Messaging.Shared.Messaging.Channel;
using Messaging.Shared.Messaging.Topology;
using Messaging.Worker.Exceptions;
using Messaging.Worker.Messaging.Retry;
using Messaging.Worker.Processors;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace Messaging.Worker.Consumers;

public class OrderCreatedConsumer : IOrderCreatedConsumer
{
    private readonly IRabbitMqChannelManager _channelManager;
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRabbitMqRetryPublisher _retryPublisher;
    private readonly IRetryPolicy _retryPolicy;
    private IChannel? _channel;
    public OrderCreatedConsumer(
        IRabbitMqChannelManager channelManager,
        IServiceScopeFactory scopeFactory,
        IRabbitMqRetryPublisher retryPublisher,
        IRetryPolicy retryPolicy,
        ILogger<OrderCreatedConsumer> logger)
    {
        _channelManager = channelManager;
        _scopeFactory = scopeFactory;
        _retryPublisher = retryPublisher;
        _retryPolicy = retryPolicy;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _channel = await _channelManager.CreateChannelAsync(cancellationToken);

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        
        consumer.ReceivedAsync += async (sender, args) =>
        {
            var body = args.Body.ToArray();

            if (args.BasicProperties.Headers is not null)
            {
                foreach (var header in args.BasicProperties.Headers)
                {
                    _logger.LogInformation(
                        "HEADER -> {Key} | Tipo: {Type} | Valor: {Value}",
                        header.Key,
                        header.Value?.GetType().FullName,
                        header.Value);
                }
            }

            var retryCount = GetRetryCount(args.BasicProperties);

            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();

                var processor = scope.ServiceProvider
                    .GetRequiredService<IOrderCreatedMessageProcessor>();
                
                await processor.ProcessAsync(
                    body,
                    cancellationToken);

                await _channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);

                _logger.LogInformation(
                    "ACK enviado para OrderCreated. " +
                    "DeliveryTag: {DeliveryTag}",
                    args.DeliveryTag);      
            }
            catch (TransientException ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao processar OrderCreated. " +
                    "DeliveryTag: {DeliveryTag} " +
                    "RetryCount: {RetryCount}",
                    args.DeliveryTag,
                    retryCount);

                if (_retryPolicy.ShouldRetry(retryCount))
                {
                    var nextRetryCount = _retryPolicy.GetNextRetryCount(retryCount);
                    var retryDelay = _retryPolicy.GetRetryDelay(retryCount);

                    await _retryPublisher.PublishAsync(
                        body,
                        nextRetryCount,
                        retryDelay,
                        cancellationToken);

                    await _channel.BasicAckAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false);

                    _logger.LogWarning(
                        "Mensagem enviada para retry. " +
                        "RetryCount: {RetryCount}, " +
                        "RetryDelay: {RetryDelay}, " +
                        "DeliveryTag: {DeliveryTag}",
                        nextRetryCount,
                        retryDelay.TotalMilliseconds,
                        args.DeliveryTag);
                }
                else
                {
                    _logger.LogWarning(
                        "Número máximo de tentativas atingido. " +
                        "Enviando mensagem para DLQ. " +
                        "RetryCount: {RetryCount}, " +
                        "DeliveryTag: {DeliveryTag}",
                        retryCount,
                        args.DeliveryTag);

                    await _channel.BasicNackAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: false);
                }
            }
            catch(RetryPublishException ex)
            {
                _logger.LogError(
                    ex,
                    "Falha ao publicar mensagem para retry. " +
                    "DeliveryTag: {DeliveryTag}" +
                    "A mensagem será reprocessada",
                    args.DeliveryTag
                );

                await _channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
            catch(PermanentException ex)
            {
                _logger.LogError(
                    ex,
                    "Erro permanente ao processar OrderCreated. " +
                    "Enviando mensagem diretamente para DLQ. " +
                    "DeliveryTag: {DeliveryTag}, " +
                    "RetryCount: {RetryCount}",
                    args.DeliveryTag,
                    retryCount);

                await _channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro não classificado ao processar OrderCreated. " +
                    "DeliveryTag: {DeliveryTag}, " +
                    "RetryCount: {RetryCount}",
                    args.DeliveryTag,
                    retryCount);
                
                await _channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: false);
            }
        };

        var consumerTag = await _channel.BasicConsumeAsync(
            consumerTag: string.Empty,
            noLocal: true,
            exclusive: false,
            arguments: null,
            queue: RabbitMqTopology.OrdersQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);
        
        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch
        {
            
        }
        finally
        {
            if (_channel.IsOpen)
            {
                await _channel.BasicCancelAsync(consumerTag);    
            }
            
            await _channel.DisposeAsync();

            _channel = null;
        }
    }

    private int GetRetryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers is not null && 
            properties.Headers.TryGetValue(RabbitMqTopology.RetryCountReader, out var retryHeader))
        {
            return Convert.ToInt32(retryHeader);
        }

        return 0;
    }
}