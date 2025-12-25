using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Infrastructure.Persistence;

//public class OrdersDbContext : DbContext
//{
//    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
//        : base(options)
//    {
//    }

//    public DbSet<Order> Orders => Set<Order>();

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
//        base.OnModelCreating(modelBuilder);
//    }
//}

public class OrdersDbContext : DbContext
{
    // 🔴 DESIGN-TIME ONLY (EF Core CLI)
    protected OrdersDbContext()
    {
    }

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
