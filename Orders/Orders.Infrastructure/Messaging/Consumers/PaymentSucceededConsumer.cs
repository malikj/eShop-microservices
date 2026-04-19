using Orders.Application.Abstractions.Repositories;
using Orders.Domain.Enums;
using eShop.Contracts.Events;
using Serilog;

namespace Orders.Infrastructure.Messaging.Consumers;

public class PaymentSucceededConsumer
{
    private readonly IOrderRepository _orders;
    private readonly IInboxRepository _inbox;

    public PaymentSucceededConsumer(
        IOrderRepository orders,
        IInboxRepository inbox)
    {
        _orders = orders;
        _inbox = inbox;
    }

    public async Task Handle(PaymentSucceeded evt, Guid messageId, Guid? correlationId, CancellationToken cancellationToken)
    {
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        using (Serilog.Context.LogContext.PushProperty("MessageId", messageId))
        using (Serilog.Context.LogContext.PushProperty("OrderId", evt.OrderId))
        {
            Log.Information("[PaymentSucceededConsumer] Processing PaymentSucceeded for OrderId: {OrderId}", evt.OrderId);

            var consumer = nameof(PaymentSucceededConsumer);

            if (await _inbox.ExistsAsync(messageId, consumer, cancellationToken))
            {
                Log.Warning("[PaymentSucceededConsumer] Duplicate message detected. MessageId: {MessageId}", messageId);
                return;
            }

            var order = await _orders.GetByIdAsync(evt.OrderId, cancellationToken);

            if (order is null || order.Status != OrderStatus.Pending)
            {
                Log.Warning("[PaymentSucceededConsumer] Order not found or not in Pending status. OrderId: {OrderId}", evt.OrderId);
                return;
            }

            order.MarkAsPaid();

            await _inbox.AddAsync(messageId, consumer, correlationId, DateTime.UtcNow, cancellationToken);
            await _orders.UpdateAsync(order, cancellationToken);

            Log.Information("[PaymentSucceededConsumer] Order marked as paid. OrderId: {OrderId}", evt.OrderId);
        }
    }
}
