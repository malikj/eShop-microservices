namespace eShop.Contracts.Events;

public record OrderCancelled(
    Guid OrderId,
    Guid CustomerId,
    DateTime CancelledAt,
    string Reason
);
