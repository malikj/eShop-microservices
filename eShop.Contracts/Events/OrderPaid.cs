namespace eShop.Contracts.Events;

public record OrderPaid(
    Guid OrderId,
    Guid CustomerId,
    DateTime PaidAt,
    decimal TotalPrice
);
