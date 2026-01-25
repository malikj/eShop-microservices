using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Application.Ordering.Dtos;

public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = default!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }
}


