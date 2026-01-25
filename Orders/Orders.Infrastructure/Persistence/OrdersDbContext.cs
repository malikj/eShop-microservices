using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orders.Infrastructure.Messaging;


namespace Orders.Infrastructure.Persistence;

public class OrdersDbContext : DbContext
{
    protected OrdersDbContext()
    {
    }

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }

    //public DbSet<Order> Orders => Set<Order>();
    public DbSet<Orders.Domain.Entities.Order> Orders => Set<Orders.Domain.Entities.Order>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
