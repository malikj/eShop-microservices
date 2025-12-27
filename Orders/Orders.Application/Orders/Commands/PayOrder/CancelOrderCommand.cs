

using MediatR;
using System;

namespace Orders.Application.Orders.Commands.PayOrder;

public record CancelOrderCommand(Guid OrderId) : IRequest;
