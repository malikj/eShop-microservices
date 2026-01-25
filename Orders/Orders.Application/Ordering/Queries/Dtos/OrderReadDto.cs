
//namespace Orders.Application.Ordering.Queries.Dtos;

//public class OrderReadDto
//{
//    public Guid Id { get; set; }
//    public Guid CustomerId { get; set; }
//    public string Status { get; set; } = default!;
//    public decimal TotalPrice { get; set; }
//    public IReadOnlyCollection<OrderItemReadDto> Items { get; set; } = new List<OrderItemReadDto>();
//}


using Orders.Domain.Enums;

namespace Orders.Application.Ordering.Queries.Dtos;

public class OrderReadDto
{
    public Guid Id { get; }
    public Guid CustomerId { get; }
    public OrderStatus Status { get; }
    public decimal TotalPrice { get; }
    public DateTime CreatedAt { get; }
    public IReadOnlyCollection<OrderItemReadDto> Items { get; }

    public OrderReadDto(
        Guid id,
        Guid customerId,
        OrderStatus status,
        decimal totalPrice,
        DateTime createdAt,
        IReadOnlyCollection<OrderItemReadDto> items)
    {
        Id = id;
        CustomerId = customerId;
        Status = status;
        TotalPrice = totalPrice;
        CreatedAt = createdAt;
        Items = items;
    }
}
