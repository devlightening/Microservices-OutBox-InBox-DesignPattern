
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using StockService.Models.Contexts;
using StockService.Models.Entites;
using System.Text.Json;

namespace StockService.Consumers
{
    public class OrderCreatedEventConsumer(StockDbContext stockDbContext) : IConsumer<OrderCreatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {

            await stockDbContext.OrderInboxes.AddAsync(new()
            {
                Processed = false,
                Payload = JsonSerializer.Serialize(context.Message)
            });

            await stockDbContext.SaveChangesAsync();

            List<OrderInbox> orderInboxes = await stockDbContext.OrderInboxes
                .Where(i => i.Processed == false)
                .ToListAsync();
            foreach (var orderInbox in orderInboxes)
            {
                OrderCreatedEvent orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(orderInbox.Payload);
                Console.WriteLine($"{orderCreatedEvent.OrderId} order id değerine karşılık olan siparişin stok işlemleri başarıyla tamamlanmıştır.");
                orderInbox.Processed = true;
                await stockDbContext.SaveChangesAsync();
            }
        }
    }
}
