using Microsoft.EntityFrameworkCore;
using Orders.Application.Abstractions.Repositories;
using Orders.Application.Orders.Queries.Dtos;
using Orders.Infrastructure.Persistence;
using Orders.Application.Orders.Queries.Dtos;


namespace Orders.Infrastructure.Repositories;

public class OrderReadRepository : IOrderReadRepository
{
    private readonly OrdersDbContext _dbContext;

    public OrderReadRepository(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<OrderReadDto>> GetAllAsync()
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Select(o => new OrderReadDto(
                o.Id,
                o.CustomerId,
                o.CreatedAt,
                o.Status.ToString(),
                o.TotalPrice,
                o.Items.Select(i => new OrderItemReadDto(
                    i.ProductId,
                    i.ProductName,
                    i.UnitPrice,
                    i.Quantity
                )).ToList()
            ))
            .ToListAsync();
    }

    public async Task<OrderReadDto?> GetByIdAsync(Guid orderId)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == orderId)
            .Select(o => new OrderReadDto(
                o.Id,
                o.CustomerId,
                o.CreatedAt,
                o.Status.ToString(),
                o.TotalPrice,
                o.Items.Select(i => new OrderItemReadDto(
                    i.ProductId,
                    i.ProductName,
                    i.UnitPrice,
                    i.Quantity
                )).ToList()
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<List<OrderReadDto>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            .Select(o => new OrderReadDto(
                o.Id,
                o.CustomerId,
                o.CreatedAt,
                o.Status.ToString(),
                o.TotalPrice,
                o.Items.Select(i => new OrderItemReadDto(
                    i.ProductId,
                    i.ProductName,
                    i.UnitPrice,
                    i.Quantity
                )).ToList()
            ))
            .ToListAsync();
    }
}
