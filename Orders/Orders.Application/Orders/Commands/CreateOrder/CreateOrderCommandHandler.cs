using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using MediatR;
using Orders.Application.Abstractions.Repositories;
using Orders.Domain.Entities;

namespace Orders.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<CreateOrderResult> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new ArgumentException("Order must contain at least one item");

        var order = new Order(
            request.OrderId,
            request.CustomerId,
            request.CreatedAt,
            request.TotalAmount);

        foreach (var item in request.Items)
        {
            order.AddItem(
                item.ProductId,
                item.ProductName,
                item.UnitPrice,
                item.Quantity);
        }

        await _orderRepository.AddAsync(order, cancellationToken);

        return new CreateOrderResult
        {
            OrderId = order.Id,
            TotalPrice = order.TotalPrice
        };
    }
}
