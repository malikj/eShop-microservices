using Orders.Application.Orders.Queries.Dtos;

namespace Orders.Application.Abstractions.Repositories;

public interface IOrderReadRepository
{
    Task<List<OrderReadDto>> GetAllAsync();
    Task<OrderReadDto?> GetByIdAsync(Guid orderId);
    Task<List<OrderReadDto>> GetByCustomerIdAsync(Guid customerId);
}
