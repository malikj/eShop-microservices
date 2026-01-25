using MediatR;
using Orders.Application.Abstractions.Repositories;
using Orders.Domain.Enums;

namespace Orders.Application.Ordering.Commands.PayOrder;

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
