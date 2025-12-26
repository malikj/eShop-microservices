


using Catalog.Application.Abstractions.Messaging;
using Catalog.Application.Checkout.Dtos;
using eShop.Contracts.Events;
using MassTransit;

namespace Catalog.Api.Messaging;

public class OrderEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderEventPublisher(IPublishEndpoint publishEndpoint)
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
        var evt = new OrderRequested(
            orderId,
            customerId,
            createdAt,
            totalAmount,
            items.ToList()
        );

        await _publishEndpoint.Publish(evt);
    }
}
