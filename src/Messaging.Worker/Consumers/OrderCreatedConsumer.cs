using System.Reflection;
using System.Text.Json;
using Messaging.Shared.Contracts;
using Messaging.Shared.Messaging.Channel;
using Messaging.Shared.Messaging.Topology;
using Messaging.Worker.Handlers;
using Messaging.Worker.Messaging.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace Messaging.Worker.Consumers;

public class OrderCreatedConsumer : IOrderCreatedConsumer
{
    private readonly IRabbitMqChannelManager _channelManager;
    private readonly IOrderCreatedHandler _handler;
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IRabbitMqRetryPublisher _retryPublisher;
    private readonly IRetryPolicy _retryPolicy;
    private IChannel? _channel;
    public OrderCreatedConsumer(
        IRabbitMqChannelManager channelManager,
        IOrderCreatedHandler handler,
        IRabbitMqRetryPublisher retryPublisher,
        IRetryPolicy retryPolicy,
        ILogger<OrderCreatedConsumer> logger)
    {
        _channelManager = channelManager;
        _handler = handler;
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


            var retryCount = 0;

            if (args.BasicProperties.Headers is not null && 
                args.BasicProperties.Headers.TryGetValue(RabbitMqTopology.RetryCountReader, out var retryHeader))
            {
                retryCount = Convert.ToInt32(retryHeader);
            }

            try
            {      
                var orderCreated = JsonSerializer.Deserialize<OrderCreated>(body);

                if (orderCreated is null)
                {
                    throw new InvalidOperationException("Não foi possível desserializar a mensagem OrderCreated");
                }

                _logger.LogInformation(
                    "OrderCreated recebida. " +
                    "ProcessId> {ProcessId}, " +
                    "DeliveryTag: {DeliveryTag}, " +
                    "Redelivered: {Redelivered}, " +
                    "OrderId: {OrderId}, " +
                    "Total: {Total}",
                    Environment.ProcessId,
                    args.DeliveryTag,
                    args.Redelivered,
                    orderCreated.OrderId,
                    orderCreated.Total);

                await _handler.HandleAsync(
                    orderCreated, 
                    cancellationToken);

                await _channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);

                _logger.LogInformation(
                    "ACK enviado para OrderCreated. " +
                    "DeliveryTag: {DeliveryTag}, " +
                    "OrderId: {OrderId}",
                    args.DeliveryTag,
                    orderCreated.OrderId);      
            }
            catch (Exception ex)
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

                    await _retryPublisher.PublishAsync(
                        body,
                        nextRetryCount,
                        cancellationToken);

                    await _channel.BasicAckAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false);

                    _logger.LogWarning(
                        "Mensagem enviada para retry. " +
                        "RetryCount: {RetryCount}, " +
                        "DeliveryTag: {DeliveryTag}",
                        nextRetryCount,
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
}