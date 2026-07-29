using Messaging.Api.Messaging.Interfaces;

namespace Messaging.Api.Messaging;

public sealed class RabbitMqBootstrapper : IHostedService
{
    private readonly IRabbitMqConnectionManager _connectionManager;
    private readonly ILogger<RabbitMqBootstrapper> _logger;

    public RabbitMqBootstrapper(
        IRabbitMqConnectionManager connectionManager,
        ILogger<RabbitMqBootstrapper> logger
    )
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Inicializando RabbitMQ...");

        await _connectionManager.GetConnectionAsync(cancellationToken);

        _logger.LogInformation("Bootstraper iniciado.");

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}