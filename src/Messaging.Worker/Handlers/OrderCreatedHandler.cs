using Messaging.Shared.Contracts;
using Messaging.Worker.Exceptions;

namespace Messaging.Worker.Handlers;

public class OrderCreatedHandler(
    ILogger<OrderCreatedHandler> logger) : IOrderCreatedHandler
{
    public async Task HandleAsync(
        OrderCreated message,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
        "INICIANDO processamento. OrderId: {OrderId}",
        message.OrderId);
        
        logger.LogInformation(
            "FINALIZANDO processamento. OrderId: {OrderId}",
            message.OrderId);

        logger.LogInformation(
            "Processando OrderCreated. " +
            "OrderId: {OrderId}, " +
            "CreatedAt: {CreatedAt}, " +
            "Total: {Total}",
            message.OrderId,
            message.CreatedAt,
            message.Total);
        
        await Task.CompletedTask;

    }
}