namespace Orders.Application.Orders.Queries.Dtos;

public record OrderReadDto(
	Guid OrderId,
	Guid CustomerId,
	DateTime CreatedAt,
	string Status,
	decimal TotalPrice,
	List<OrderItemReadDto> Items
);
