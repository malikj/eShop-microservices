using MediatR;

namespace Orders.Application.Orders.Commands.PayOrder;

public record PayOrderCommand(Guid OrderId) : IRequest;
