using Messaging.Shared.Messaging.Channel;
using Messaging.Shared.Messaging.Connection;
using Messaging.Shared.Messaging.Topology;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqInfrastructure(
        this IServiceCollection services)
    {
        services.AddSingleton<IRabbitMqConnectionManager, RabbitMqConnectionManager>();
        services.AddSingleton<IRabbitMqChannelManager, RabbitMqChannelManager>();
        services.AddSingleton<IRabbitMqTopologyInitializer, RabbitMqTopologyInitializer>();
        
        return services;
    }
}