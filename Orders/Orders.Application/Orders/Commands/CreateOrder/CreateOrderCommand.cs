using Orders.Application.Orders.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using Orders.Application.Orders.Dtos;

namespace Orders.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommand : IRequest<CreateOrderResult>
{
    public Guid CustomerId { get; init; }
    public List<CreateOrderItemDto> Items { get; init; } = new();
}

