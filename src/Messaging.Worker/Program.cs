using Messaging.Shared.Extensions;
using Messaging.Shared.Messaging.Configuration;
using Messaging.Worker;
using Messaging.Worker.Consumers;
using Messaging.Worker.Handlers;
using Messaging.Worker.Messaging.Retry;

var builder = Host.CreateApplicationBuilder(args);

//RabbitMQ
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddRabbitMqInfrastructure();

builder.Services.AddSingleton<IRabbitMqRetryPublisher, RabbitMqRetryPublisher>();
builder.Services.AddSingleton<IRetryPolicy, RetryPolicy>();

builder.Services.AddSingleton<IOrderCreatedConsumer, OrderCreatedConsumer>();
builder.Services.AddSingleton<IOrderCreatedHandler, OrderCreatedHandler>();


builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
