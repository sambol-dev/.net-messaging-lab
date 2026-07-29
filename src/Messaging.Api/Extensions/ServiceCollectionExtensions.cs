using Messaging.Api.Configuration;
using Messaging.Api.Messaging;
using Messaging.Api.Messaging.Interfaces;

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

        services.AddHostedService<RabbitMqBootstrapper>();
        return services;
    }
}