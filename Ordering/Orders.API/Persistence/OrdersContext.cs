using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Orders.API.Models;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.API.Persistence
{
    public class OrdersContext : DbContext
    {

        public OrdersContext(DbContextOptions<OrdersContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var converter = new EnumToStringConverter<Status>();
            builder
                .Entity<Order>()
                .Property(p => p.Status)
                .HasConversion(converter);
        }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        public void MigrateDB()
        {
            // add this package to the project <PackageReference Include="Polly" Version="7.2.1" />
            //and import the namespace Polly
            Policy
                .Handle<Exception>()
                .WaitAndRetry(10, r => TimeSpan.FromSeconds(10))
                .Execute(() => Database.Migrate());
        }
    }
}
