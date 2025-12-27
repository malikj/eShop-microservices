using MediatR;
using Orders.Application.Abstractions.Repositories;
using Orders.Domain.Enums;

namespace Orders.Application.Orders.Commands.PayOrder;

public class CancelOrderCommandHandler
    : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _repo;

    public CancelOrderCommandHandler(IOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _repo.GetByIdAsync(
            request.OrderId,
            cancellationToken)
            ?? throw new Exception("Order not found");

        if (order.Status == OrderStatus.Shipped)
            throw new InvalidOperationException("Cannot cancel shipped order");

        order.Cancel();

        await _repo.UpdateAsync(order, cancellationToken);
    }
}



//using MediatR;
//using Orders.Application.Abstractions.Messaging;
//using Orders.Application.Abstractions.Repositories;
//using eShop.Contracts.Events;

//namespace Orders.Application.Orders.Commands.PayOrder;

//public class CancelOrderCommandHandler
//    : IRequestHandler<CancelOrderCommand>
//{
//    private readonly IOrderRepository _repo;
//    private readonly IEventPublisher _publisher;

//    public CancelOrderCommandHandler(
//        IOrderRepository repo,
//        IEventPublisher publisher)
//    {
//        _repo = repo;
//        _publisher = publisher;
//    }

//    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
//    {
//        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
//            ?? throw new InvalidOperationException("Order not found");

//        order.Cancel();

//        await _orderRepository.UpdateAsync(order, cancellationToken);

//        await _publishEndpoint.Publish(new OrderCancelled(
//            order.Id,
//            order.CustomerId,
//            DateTime.UtcNow,
//            request.Reason
//        ), cancellationToken);
//    }
//}
