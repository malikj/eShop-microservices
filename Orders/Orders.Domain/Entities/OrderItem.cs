using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Domain.Entities;

public class OrderItem
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem() { } // EF Core

    public OrderItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId is required");

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("ProductName is required");

        if (unitPrice <= 0)
            throw new ArgumentException("UnitPrice must be greater than zero");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public decimal GetTotal() => UnitPrice * Quantity;
}
