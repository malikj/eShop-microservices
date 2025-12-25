using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Infrastructure.Persistence;

public class OrdersDbContextFactory
    : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdersDbContext>();

        optionsBuilder.UseSqlServer(
    "Server=localhost,1435;Database=OrdersDb;User Id=sa;Password=SqlDev@123;TrustServerCertificate=True");

        return new OrdersDbContext(optionsBuilder.Options);
    }
}
