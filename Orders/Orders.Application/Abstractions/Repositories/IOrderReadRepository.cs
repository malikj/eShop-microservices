//using Orders.Application.Orders.Queries.Dtos;

//namespace Orders.Application.Abstractions.Repositories;

//public interface IOrderReadRepository
//{
//    Task<List<OrderReadDto>> GetAllAsync();
//    Task<OrderReadDto?> GetByIdAsync(Guid orderId);
//    Task<List<OrderReadDto>> GetByCustomerIdAsync(Guid customerId);
//}

using Orders.Application.Ordering.Queries.Dtos;

namespace Orders.Application.Abstractions.Repositories;

public interface IOrderReadRepository
{
    Task<IReadOnlyList<OrderReadDto>> GetAllAsync();
    Task<IReadOnlyList<OrderReadDto>> GetByCustomerIdAsync(Guid customerId);
    Task<OrderReadDto?> GetByIdAsync(Guid orderId);
}
