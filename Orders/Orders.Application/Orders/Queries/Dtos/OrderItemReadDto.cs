namespace Orders.Application.Orders.Queries.Dtos;

public record OrderItemReadDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity
);


