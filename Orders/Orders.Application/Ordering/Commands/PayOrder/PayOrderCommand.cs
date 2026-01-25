using MediatR;

namespace Orders.Application.Ordering.Commands.PayOrder;

public record PayOrderCommand(Guid OrderId) : IRequest;
