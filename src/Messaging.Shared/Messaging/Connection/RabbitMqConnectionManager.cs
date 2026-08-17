using Messaging.Shared.Messaging.Configuration;
using RabbitMQ.Client;
using Microsoft.Extensions.Options;

namespace Messaging.Shared.Messaging.Connection;

public sealed class RabbitMqConnectionManager : IRabbitMqConnectionManager
{
    private readonly RabbitMqOptions _options;
    private IConnection? _connection;

    private readonly SemaphoreSlim _semaphore = new(1,1);

    public RabbitMqConnectionManager(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is { IsOpen: true })
            return _connection;
        
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                VirtualHost = _options.VirtualHost,
                UserName = _options.Username,
                Password = _options.Password,
                ClientProvidedName = "Messaging API"
            };    

            _connection = await factory.CreateConnectionAsync(cancellationToken);

            return _connection;
        }
        finally
        {
            _semaphore.Release();
        }        
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _semaphore.Dispose();
    }
}