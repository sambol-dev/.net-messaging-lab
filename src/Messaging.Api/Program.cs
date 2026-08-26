using Messaging.Shared.Extensions;
using Messaging.Api.Messaging.Publisher;
using Messaging.Shared.Messaging.Topology;
using Messaging.Shared.Contracts;
using Messaging.Shared.Messaging.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//RabbitMQ
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddRabbitMqInfrastructure();

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

//Endpoint teste
app.MapPost("orders", async (
    IRabbitMqPublisher publisher,
    CancellationToken cancellationToken) =>
{
    var message = new OrderCreated(
        Guid.NewGuid(),
        DateTime.UtcNow,
        199.90m);

    await publisher.PublishAsync(
        message,
        RabbitMqTopology.OrdersExchange,
        RabbitMqTopology.OrdersRoutingKey,
        cancellationToken);
    
    return Results.Accepted();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
