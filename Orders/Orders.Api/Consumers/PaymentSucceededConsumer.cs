using MassTransit;
using Orders.Application.Abstractions.Repositories;
using Orders.Domain.Enums;
using eShop.Contracts.Events;
using Serilog;

namespace Orders.Api.Consumers;
public class PaymentSucceededConsumer : IConsumer<PaymentSucceeded>
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

    public async Task Consume(ConsumeContext<PaymentSucceeded> context)
    {
        using (Serilog.Context.LogContext.PushProperty(
            "CorrelationId", context.CorrelationId))
        using (Serilog.Context.LogContext.PushProperty(
            "MessageId", context.MessageId))
        using (Serilog.Context.LogContext.PushProperty(
            "OrderId", context.Message.OrderId))
        {
            Log.Information("Processing PaymentSucceeded");

            var messageId = context.MessageId!.Value;
            var consumer = nameof(PaymentSucceededConsumer);

            if (await _inbox.ExistsAsync(messageId, consumer, context.CancellationToken))
                return;

            var order = await _orders.GetByIdAsync(
                context.Message.OrderId,
                context.CancellationToken);

            if (order is null || order.Status != OrderStatus.Pending)
                return;

            order.MarkAsPaid();

            await _inbox.AddAsync(
                messageId,
                consumer,
                context.CorrelationId,
                DateTime.UtcNow,
                context.CancellationToken);

            await _orders.UpdateAsync(order, context.CancellationToken);
        }
    }


    //public async Task Consume(ConsumeContext<PaymentSucceeded> context)
    //{
    //    using (Serilog.Context.LogContext.PushProperty("CorrelationId", context.CorrelationId))
    //    using (Serilog.Context.LogContext.PushProperty("MessageId", context.MessageId))
    //    using (Serilog.Context.LogContext.PushProperty("OrderId", context.Message.OrderId))
    //    {
          
    //        // normal logic

    //        Console.WriteLine(" ENTERED PaymentSucceededConsumer ");

    //        var messageId = context.MessageId!.Value;
    //        Console.WriteLine($"MessageId: {messageId}");

    //        var consumer = nameof(PaymentSucceededConsumer);

    //        Console.WriteLine(" Checking inbox");

    //        if (await _inbox.ExistsAsync(messageId, consumer, context.CancellationToken))
    //            return;

    //        Console.WriteLine(" Not duplicate, continuing");

    //        var order = await _orders.GetByIdAsync(
    //            context.Message.OrderId,
    //            context.CancellationToken);

    //        if (order is null)
    //            return;

    //        if (order.Status != OrderStatus.Pending)
    //            return; //  IMPORTANT: ignore, DO NOT throw

    //        order.MarkAsPaid();

    //        Console.WriteLine(" Adding InboxMessage");

    //        //  Add inbox record BEFORE SaveChanges
         
    //  await _inbox.AddAsync(context.MessageId!.Value,
    //nameof(PaymentSucceededConsumer),
    //context.CorrelationId,
    //DateTime.UtcNow,
    //context.CancellationToken);

    //        //  Single SaveChanges happens here
    //        await _orders.UpdateAsync(order, context.CancellationToken);
    //    }
    //}
}
