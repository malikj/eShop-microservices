//using Catalog.Application.Abstractions.Messaging;
//using MassTransit;

//namespace Catalog.Infrastructure.Messaging;

//public class MassTransitEventPublisher : IEventPublisher
//{
//    private readonly IPublishEndpoint _publishEndpoint;

//    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
//    {
//        _publishEndpoint = publishEndpoint;
//    }

//    public async Task PublishAsync<T>(T message)
//        where T : class
//    {
//        await _publishEndpoint.Publish(message);
//    }
//}

using Catalog.Application.Abstractions.Messaging;
using eShop.Contracts.Events;
using MassTransit;

namespace Catalog.Infrastructure.Messaging;

public class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishOrderRequestedAsync(
        Guid orderId,
        Guid customerId,
        DateTime createdAt,
        decimal totalAmount,
        IReadOnlyList<OrderItemDto> items)
    {
        var orderRequested = new OrderRequested(
            orderId,
            customerId,
            createdAt,
            totalAmount,
            items.ToList()
        );

        await _publishEndpoint.Publish(orderRequested);
    }
}
