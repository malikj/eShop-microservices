namespace eShop.Contracts.Events;

public record OrderPaymentRequested(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount
);
