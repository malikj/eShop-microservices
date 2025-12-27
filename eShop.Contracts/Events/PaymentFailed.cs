namespace eShop.Contracts.Events;

public record PaymentFailed(
    Guid OrderId,
    string Reason
);
