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
        Console.WriteLine(" ENTERED OrderPaymentRequestedConsumer");

        var message = context.Message;

		var success = message.Amount <= 100_0000;

		if (success)
		{
            Console.WriteLine(" Publishing PaymentSucceeded");

            await _publishEndpoint.Publish(new PaymentSucceeded(
				message.OrderId,
				DateTime.UtcNow
			));
		}
		else
		{
            Console.WriteLine(" Publishing PaymentFailed");

            await _publishEndpoint.Publish(new PaymentFailed(
				message.OrderId,
				"Amount exceeds allowed limit"
			));
		}
	}
}
