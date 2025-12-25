using MassTransit;
using eShop.Contracts.Events;

namespace Catalog.Api.Messaging;

public class OrderEventPublisher
{
	private readonly IPublishEndpoint _publishEndpoint;

	public OrderEventPublisher(IPublishEndpoint publishEndpoint)
	{
		_publishEndpoint = publishEndpoint;
	}

	public async Task PublishOrderRequestedAsync()
	{
		var orderEvent = new OrderRequested(
			Guid.NewGuid(),
			Guid.NewGuid(),
			new List<OrderItemDto>
			{
				new(
					Guid.NewGuid(),
					"Sample Product",
					99.99m,
					1
				)
			}
		);

		await _publishEndpoint.Publish(orderEvent);
	}
}
