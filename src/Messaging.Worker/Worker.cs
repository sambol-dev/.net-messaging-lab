using Messaging.Shared.Messaging.Topology;
using Messaging.Worker.Consumers;
namespace Messaging.Worker;

public class Worker(
    ILogger<Worker> logger,
    IRabbitMqTopologyInitializer topologyInitializer,
    IOrderCreatedConsumer orderCreatedConsumer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker iniciando. Processo {ProcessId}",
        Environment.ProcessId);

        await topologyInitializer.InitializeAsync(stoppingToken);
        logger.LogInformation("Topologia RabbitMq iniciada.");

        await orderCreatedConsumer.StartAsync(stoppingToken);
        logger.LogInformation("OrderCreatedConsumer iniciado.");
    }
}
