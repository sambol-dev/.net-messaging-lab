using System.Text.Json;
using Messaging.Shared.Contracts;
using Messaging.Worker.Exceptions;
using Messaging.Worker.Handlers;
using Messaging.Worker.Idempotency;

namespace Messaging.Worker.Processors;

public class OrderCreatedMessageProcessor(
    IOrderCreatedHandler handler,
    IProcessedMessageStore processedMessageStore,
    ILogger<OrderCreatedMessageProcessor> logger)
    : IOrderCreatedMessageProcessor
{
    public async Task ProcessAsync(
        byte[] body,
        CancellationToken cancellationToken)
    {
        var orderCreated = DeserializeMessage(body);

        logger.LogInformation(
            "Processando OrderCreated. " +
            "OrderId: {OrderId}, " +
            "CreatedAt: {CreatedAt}, " +
            "Total: {Total}",
            orderCreated.OrderId,
            orderCreated.CreatedAt,
            orderCreated.Total
        );

        var messageId = orderCreated.OrderId;

        var alreadyProcessed = await processedMessageStore.ExistsAsync(
            messageId,
            "OrderCreated",
            cancellationToken);

        if (alreadyProcessed)
        {
            logger.LogInformation(
                "Mensagem duplicada ignorada. " +
                "MessageId: {MessageId}",
                messageId);
            
            return;
        }

        await handler.HandleAsync(
            orderCreated,
            cancellationToken);
        
        await processedMessageStore.MarkAsProcessedAsync(
            messageId,
            "OrderCreated",
            cancellationToken);
    }

    private OrderCreated DeserializeMessage(byte[] body)
    {
        try
        {
            var orderCreated =
                JsonSerializer.Deserialize<OrderCreated>(body);

            if (orderCreated is null)
            {
                throw new PermanentException(
                    "Não foi possível desserializar a mensagem OrderCreated");
            }

            return orderCreated;
        }
        catch (JsonException ex)
        {
            throw new PermanentException(
                "A mensagem OrderCreated contém um Json inválido",
                ex);
        }
    }
}