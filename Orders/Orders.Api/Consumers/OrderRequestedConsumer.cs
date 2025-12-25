using MassTransit;
using MediatR;
using eShop.Contracts.Events;
using Orders.Application.Orders.Commands.CreateOrder;
using Orders.Application.Orders.Dtos;

namespace Orders.Api.Consumers;

public class OrderRequestedConsumer : IConsumer<OrderRequested>
{
	private readonly IMediator _mediator;

	public OrderRequestedConsumer(IMediator mediator)
	{
		_mediator = mediator;
	}

	public async Task Consume(ConsumeContext<OrderRequested> context)
	{
		var evt = context.Message;

		var command = new CreateOrderCommand
		{
			CustomerId = evt.CustomerId,
			Items = evt.Items.Select(i => new CreateOrderItemDto
			{
				ProductId = i.ProductId,
				ProductName = i.ProductName,
				UnitPrice = i.UnitPrice,
				Quantity = i.Quantity
			}).ToList()
		};

		await _mediator.Send(command);
	}
}
