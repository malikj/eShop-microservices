using Microsoft.EntityFrameworkCore;
using Orders.Application.Abstractions.Repositories;
using Orders.Domain.Entities;
using Orders.Infrastructure.Persistence;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Repositories;
public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _dbContext;

    public OrderRepository(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
        Console.WriteLine($"📦 OrderRepository DbContext hash: {_dbContext.GetHashCode()}");
    }

    public async Task AddAsync(Orders.Domain.Entities.Order order, CancellationToken cancellationToken)
    {
        await _dbContext.Orders.AddAsync(order, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Orders.Domain.Entities.Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task UpdateAsync(Orders.Domain.Entities.Order order, CancellationToken cancellationToken)
    {
        _dbContext.Orders.Update(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
