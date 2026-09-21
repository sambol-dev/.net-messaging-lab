using Messaging.Shared.Extensions;
using Messaging.Shared.Messaging.Configuration;
using Messaging.Worker;
using Messaging.Worker.Consumers;
using Messaging.Worker.Data;
using Messaging.Worker.Handlers;
using Messaging.Worker.Idempotency;
using Messaging.Worker.Messaging.Retry;
using Messaging.Worker.Processors;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

//RabbitMQ
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddRabbitMqInfrastructure();

builder.Services.AddSingleton<IRabbitMqRetryPublisher, RabbitMqRetryPublisher>();
builder.Services.AddSingleton<IRetryPolicy, RetryPolicy>();

//SQLite
builder.Services.AddDbContext<MessagingDbContext>(options => options.UseSqlite("Data Source=messaging.db"));
builder.Services.AddScoped<IProcessedMessageStore, SqliteProcessedMessageStore>();

//OrderCreated
builder.Services.AddSingleton<IOrderCreatedConsumer, OrderCreatedConsumer>();
builder.Services.AddSingleton<IOrderCreatedHandler, OrderCreatedHandler>();
builder.Services.AddScoped<IOrderCreatedMessageProcessor, OrderCreatedMessageProcessor>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
