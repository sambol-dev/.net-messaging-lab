using Messaging.Shared.Messaging.Configuration;
using Messaging.Api.Messaging.Bootstrap;
using Messaging.Shared.Messaging.Channel;
using Messaging.Shared.Messaging.Connection;
using Messaging.Api.Messaging.Publisher;
using Messaging.Api.Messaging.Topology;

namespace Messaging.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<RabbitMqOptions>(
            configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddSingleton<IRabbitMqConnectionManager, RabbitMqConnectionManager>();
        services.AddSingleton<IRabbitMqChannelManager, RabbitMqChannelManager>();
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        services.AddSingleton<IRabbitMqTopologyInitializer,RabbitMqTopologyInitializer>();
        
        services.AddHostedService<RabbitMqBootstrapper>();
        return services;
    }
}