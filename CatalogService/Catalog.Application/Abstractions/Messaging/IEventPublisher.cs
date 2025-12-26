using Catalog.Application.Checkout.Dtos;
using eShop.Contracts.Events;

namespace Catalog.Application.Abstractions.Messaging;

public interface IEventPublisher
{
    Task PublishOrderRequestedAsync(
        Guid orderId,
        Guid customerId,
        DateTime createdAt,
        decimal totalAmount,
        IReadOnlyList<OrderItemDto> items);
}
