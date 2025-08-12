using Microsoft.EntityFrameworkCore;
using StockService.Models.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockService.Models.Contexts
{
    public class StockDbContext : DbContext
    {
        public StockDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<OrderInbox> OrderInboxes { get; set; }
    }
}
