using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderOutboxTablePublisherService.Jobs
{
    public class OrderOutboxPublishJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
             Console.WriteLine("Tetiklendi.." + DateTime.UtcNow.Second);
        }
    }
}
