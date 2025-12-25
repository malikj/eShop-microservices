namespace eShop.Contracts.Events;

public record OrderRequested
(
    Guid OrderId,
    Guid CustomerId,
    DateTime CreatedAt,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items
);

public record OrderItemDto
(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity
);
