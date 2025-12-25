using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Application.Orders.Commands.CreateOrder;

public class CreateOrderResult
{
    public Guid OrderId { get; init; }
    public decimal TotalPrice { get; init; }
}
