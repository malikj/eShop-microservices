

using MassTransit;
using Orders.Application.Abstractions.Repositories;
using Orders.Domain.Enums;
using eShop.Contracts.Events;


namespace Orders.Api.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailed>
{
    private readonly IOrderRepository _repo;

    public PaymentFailedConsumer(IOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task Consume(ConsumeContext<PaymentFailed> context)
    {
        var order = await _repo.GetByIdAsync(
            context.Message.OrderId,
            context.CancellationToken)
            ?? throw new Exception("Order not found");

        order.Cancel();

        await _repo.UpdateAsync(order, context.CancellationToken);
    }
}
