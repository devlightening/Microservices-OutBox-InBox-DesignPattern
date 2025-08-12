using MassTransit;
using OrderOutboxTablePublisherService;
using OrderOutboxTablePublisherService.Entites;
using Quartz;
using Shared.Events;
using System.Text.Json;

namespace Order.Outbox.Table.Publisher.Service.Jobs
{
    public class OrderOutboxPublishJob(IPublishEndpoint publishEndpoint) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            // Veritabanı durumu kontrolü ve meşgul etme/serbest bırakma
            if (OrderOutboxSingletonDatabase.DataReaderState)
            {
                OrderOutboxSingletonDatabase.DataReaderBusy();

                // Doğru sütun adıyla (ProcessedDate) sorgu yapma
                List<OrderOutbox> orderOutboxes = (await OrderOutboxSingletonDatabase.QueryAsync<OrderOutbox>($@"SELECT * FROM ORDEROUTBOXES WHERE ProcessedDate IS NULL ORDER BY OccuredOn ASC")).ToList();

                foreach (var orderOutbox in orderOutboxes)
                {
                    // Yalnızca OrderCreatedEvent tipindeki mesajları işle
                    if (orderOutbox.Type == nameof(OrderCreatedEvent))
                    {
                        OrderCreatedEvent? orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(orderOutbox.Payload);
                        if (orderCreatedEvent != null)
                        {
                            await publishEndpoint.Publish(orderCreatedEvent);

                            // Güncelleme işlemini güvenli bir şekilde yapma
                            await OrderOutboxSingletonDatabase.ExecuteAsync($"UPDATE ORDEROUTBOXES SET ProcessedDate = GETDATE() WHERE IdempotentToken = '{orderOutbox.IdempotentToken}'");
                        }
                    }
                }

                OrderOutboxSingletonDatabase.DataReaderReady();
                await Console.Out.WriteLineAsync("Order outbox table checked!");
            }
        }
    }
}