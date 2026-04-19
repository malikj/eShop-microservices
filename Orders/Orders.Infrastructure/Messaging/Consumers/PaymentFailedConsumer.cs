using Orders.Application.Abstractions.Repositories;
using Orders.Domain.Enums;
using eShop.Contracts.Events;
using Serilog;

namespace Orders.Infrastructure.Messaging.Consumers;

public class PaymentFailedConsumer
{
    private readonly IOrderRepository _repo;

    public PaymentFailedConsumer(IOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task Handle(PaymentFailed evt, CancellationToken cancellationToken)
    {
        Log.Information("[PaymentFailedConsumer] Processing PaymentFailed for OrderId: {OrderId}", evt.OrderId);

        var order = await _repo.GetByIdAsync(evt.OrderId, cancellationToken);

        if (order is null)
        {
            Log.Warning("[PaymentFailedConsumer] Order not found. OrderId: {OrderId}", evt.OrderId);
            return;
        }

        order.Cancel();
        await _repo.UpdateAsync(order, cancellationToken);

        Log.Information("[PaymentFailedConsumer] Order cancelled. OrderId: {OrderId}", evt.OrderId);
    }
}
