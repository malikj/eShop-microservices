namespace eShop.Contracts.Events;

public record PaymentSucceeded(
    Guid OrderId,
    DateTime PaidAt
);
