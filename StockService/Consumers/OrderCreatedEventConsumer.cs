using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using StockService.Models.Contexts;
using StockService.Models.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StockService.Consumers
{
    public class OrderCreatedEventConsumer(StockDbContext stockDbContext) : IConsumer<OrderCreatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var result = await stockDbContext.OrderInboxes.AnyAsync(i => i.IdempotentToken == context.Message.IdempotentToken);
            if (!result)
            {
                await stockDbContext.OrderInboxes.AddAsync(new()
                {
                    Processed = false,
                    Payload = JsonSerializer.Serialize(context.Message),
                    IdempotentToken = context.Message.IdempotentToken
                });
                await stockDbContext.SaveChangesAsync();
            }


            List<OrderInbox> orderInboxes = await stockDbContext.OrderInboxes
             .Where(i => i.Processed == false)
             .ToListAsync();

            foreach (var orderInbox in orderInboxes)
            {
                OrderCreatedEvent orderCreatedEvent  = JsonSerializer.Deserialize<OrderCreatedEvent>(orderInbox.Payload);
                Console.WriteLine($"{orderCreatedEvent.OrderId} Order ID değerine sahip sipariş alındı.Stok işlemleri yapıldı ve  Siparişin toplam tutarı: {orderCreatedEvent.TotalPrice} TL");
                orderInbox.Processed = true;
                await stockDbContext.SaveChangesAsync();
            }
        }
    }
}
