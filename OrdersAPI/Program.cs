using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrdersAPI.Consumer;
using OrdersAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("OrdersDatabase"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));
builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<ProductCreatedConsumer>();
    configurator.UsingRabbitMq((context, rabbitMq) =>
    {
        rabbitMq.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", host =>
        {
            host.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            host.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        rabbitMq.ReceiveEndpoint("event-listener", endpoint =>
        {
            endpoint.ConfigureConsumer<ProductCreatedConsumer>(context);
        });
    });
});
builder.Services.AddOpenApi();

var app = builder.Build();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
