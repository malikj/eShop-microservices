using Orders.Application.Ordering.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace Orders.Application.Ordering.Commands.CreateOrder;

public class CreateOrderCommand : IRequest<CreateOrderResult>
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime CreatedAt { get; init; }
    public decimal TotalAmount { get; init; }

    public List<CreateOrderItemDto> Items { get; init; } = new();
}
