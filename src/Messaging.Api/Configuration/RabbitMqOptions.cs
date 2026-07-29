namespace Messaging.Api.Configuration;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public required string Host { get; init; }

    public required int Port { get; init; }

    public required string VirtualHost { get; init; }

    public required string Username { get; init; }

    public required string Password { get; init; }
}
