
using Catalog.Application.Abstractions.Messaging;
using eShop.Contracts.Events;

namespace Catalog.Infrastructure.Messaging;

//public class DummyEventPublisher : IEventPublisher
//{
//    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
//        where T : class
//    {
//        // Do nothing (for Azure testing)
//        return Task.CompletedTask;
//    }
//}

public class DummyEventPublisher : IEventPublisher
{
    public Task PublishOrderRequestedAsync(
        Guid orderId,
        Guid userId,
        DateTime orderDate,
        decimal totalAmount,
        IReadOnlyList<OrderItemDto> items)
    {
        // Do nothing (temporary for Azure deployment)
        Console.WriteLine("DummyEventPublisher called - no-op");

        return Task.CompletedTask;
    }
}