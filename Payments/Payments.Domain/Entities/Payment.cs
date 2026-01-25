namespace Payments.Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsSuccessful { get; private set; }

    private Payment() { }

    public Payment(Guid orderId, decimal amount)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkSuccess() => IsSuccessful = true;
    public void MarkFailed() => IsSuccessful = false;
}
