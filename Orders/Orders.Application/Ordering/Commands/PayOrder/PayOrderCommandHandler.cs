using MediatR;
using Orders.Application.Abstractions.Messaging;
using Orders.Application.Abstractions.Repositories;
using eShop.Contracts.Events;
using Orders.Domain.Enums;
using System.Diagnostics;


namespace Orders.Application.Ordering.Commands.PayOrder;

public class PayOrderCommandHandler
    : IRequestHandler<PayOrderCommand>
{
    private readonly IOrderRepository _repo;
    private readonly IEventPublisher _publisher;

    public PayOrderCommandHandler(
        IOrderRepository repo,
        IEventPublisher publisher)
    {
        _repo = repo;
        _publisher = publisher;
    }

    public async Task Handle(
     PayOrderCommand request,
     CancellationToken cancellationToken)
    {
        Console.WriteLine($"Handler TraceId: {Activity.Current?.TraceId}");

        var order = await _repo.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new Exception("Order not found");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Order is not payable");

        await _publisher.PublishAsync(
            new OrderPaymentRequested(
                order.Id,
                order.CustomerId,
                order.TotalPrice
            ));
    }

}
