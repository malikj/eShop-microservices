using MassTransit;
using eShop.Contracts.Events;

namespace Payments.Api.Consumers;
public class OrderPaymentRequestedConsumer
	: IConsumer<OrderPaymentRequested>
{
	private readonly IPublishEndpoint _publishEndpoint;

	public OrderPaymentRequestedConsumer(IPublishEndpoint publishEndpoint)
	{
		_publishEndpoint = publishEndpoint;
	}

	public async Task Consume(ConsumeContext<OrderPaymentRequested> context)
	{
		var message = context.Message;

		var success = message.Amount <= 100_000;

		if (success)
		{
			await _publishEndpoint.Publish(new PaymentSucceeded(
				message.OrderId,
				DateTime.UtcNow
			));
		}
		else
		{
			await _publishEndpoint.Publish(new PaymentFailed(
				message.OrderId,
				"Amount exceeds allowed limit"
			));
		}
	}
}
