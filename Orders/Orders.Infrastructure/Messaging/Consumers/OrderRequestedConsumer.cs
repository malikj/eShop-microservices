using MediatR;
using Microsoft.Extensions.Logging;
using eShop.Contracts.Events;
using Orders.Application.Ordering.Commands.CreateOrder;
using Orders.Application.Ordering.Dtos;

namespace Orders.Infrastructure.Messaging.Consumers;

public class OrderRequestedConsumer
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderRequestedConsumer> _logger;

    public OrderRequestedConsumer(IMediator mediator, ILogger<OrderRequestedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(OrderRequested evt)
    {
        _logger.LogInformation("[OrderRequestedConsumer] Handling OrderRequested event. OrderId: {OrderId}, CustomerId: {CustomerId}, TotalAmount: {TotalAmount}",
            evt.OrderId, evt.CustomerId, evt.TotalAmount);

        var command = new CreateOrderCommand
        {
            OrderId = evt.OrderId,
            CustomerId = evt.CustomerId,
            CreatedAt = evt.CreatedAt,
            TotalAmount = evt.TotalAmount,
            Items = evt.Items.Select(i => new CreateOrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        };

        _logger.LogInformation("[OrderRequestedConsumer] Sending CreateOrderCommand via MediatR. OrderId: {OrderId}", evt.OrderId);

        var result = await _mediator.Send(command);

        _logger.LogInformation("[OrderRequestedConsumer] Order created successfully. OrderId: {OrderId}, TotalPrice: {TotalPrice}",
            result.OrderId, result.TotalPrice);
    }
}
