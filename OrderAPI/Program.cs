using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Models.Contexts;
using OrderAPI.Models.Entities;
using OrderAPI.ViewModels;
using Shared.Events;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);

    });
});



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/create-order", async (CreateOrderViewModel createOrderViewModel, OrderDbContext orderDbContext) =>
{
    Order order = new()
    {
        BuyerId = createOrderViewModel.BuyerId,
        TotalPrice = createOrderViewModel.OrderItems.Sum(oi => oi.Price * oi.Count),
        OrderItems = createOrderViewModel.OrderItems.Select(i => new OrderItem
        {
            ProductId = i.ProductId,
            Count = i.Count,
            Price = i.Price
        }).ToList()

    };

    await orderDbContext.Orders.AddAsync(order);
    await orderDbContext.SaveChangesAsync();

    var idempotentToken = Guid.NewGuid();
    OrderCreatedEvent orderCreatedEvent = new()
    {
        OrderId = order.Id,
        BuyerId = order.BuyerId,
        TotalPrice = createOrderViewModel.OrderItems.Sum(oi => oi.Count * oi.Price),
        OrderItems = order.OrderItems.Select(i => new Shared.Datas.OrderItem
        {
            ProductId = i.ProductId,
            Count = i.Count,
            Price = i.Price
        }).ToList(),
        IdempotentToken = idempotentToken
    };

    #region Outbox Pattern Olmaksýzýn!
    //var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEvent}"));
    //await sendEndpoint.Send<OrderCreatedEvent>(orderCreatedEvent);
    #endregion

    #region Outbox Pattern varsa..
    OrderOutbox orderOutbox = new()
    {
        OccuredOn = DateTime.UtcNow,
        ProcessedDate = null,
        Payload = JsonSerializer.Serialize(orderCreatedEvent),
        Type = nameof(OrderCreatedEvent),
        IdempotentToken = idempotentToken
    };

    await orderDbContext.OrderOutboxes.AddAsync(orderOutbox);
    await orderDbContext.SaveChangesAsync();
    #endregion


});




app.Run();
