using MassTransit;
using Orders.Application.Abstractions.Repositories;
using Orders.Domain.Enums;
using eShop.Contracts.Events;

namespace Orders.Api.Consumers;
public class PaymentSucceededConsumer : IConsumer<PaymentSucceeded>
{
    private readonly IOrderRepository _repo;

    public PaymentSucceededConsumer(IOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task Consume(ConsumeContext<PaymentSucceeded> context)
    {
        var order = await _repo.GetByIdAsync(
            context.Message.OrderId,
            CancellationToken.None)
            ?? throw new Exception("Order not found");

        order.MarkAsPaid();

        await _repo.UpdateAsync(order, CancellationToken.None);
    }
}
