
using MediatR;
using System;

namespace Orders.Application.Ordering.Commands.PayOrder;

public record CancelOrderCommand(Guid OrderId) : IRequest;
