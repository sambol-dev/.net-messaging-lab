using Messaging.Api.Messaging.Channel;
using Messaging.Api.Messaging.Connection;
using Messaging.Api.Messaging.Topology;

namespace Messaging.Api.Messaging.Bootstrap;

public sealed class RabbitMqBootstrapper : IHostedService
{
    private readonly IRabbitMqConnectionManager _connectionManager;
    private readonly IRabbitMqTopologyInitializer _topologyInitializer;
    private readonly ILogger<RabbitMqBootstrapper> _logger;

    public RabbitMqBootstrapper(
        IRabbitMqConnectionManager connectionManager,
        IRabbitMqTopologyInitializer topologyInitializer,
        ILogger<RabbitMqBootstrapper> logger
    )
    {
        _connectionManager = connectionManager;
        _topologyInitializer = topologyInitializer;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Inicializando RabbitMQ...");

        await _connectionManager.GetConnectionAsync(cancellationToken);

        await _topologyInitializer.InitializeAsync(cancellationToken);
        
        _logger.LogInformation("RabbitMQ iniciado.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}