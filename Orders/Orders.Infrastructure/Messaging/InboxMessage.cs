namespace Orders.Infrastructure.Messaging;

public class InboxMessage
{
    public Guid MessageId { get; set; }
    public string Consumer { get; set; } = default!;
    public Guid? CorrelationId { get; set; }
    public DateTime ProcessedAt { get; set; }
}
