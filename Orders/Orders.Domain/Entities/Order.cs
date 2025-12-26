using Orders.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public decimal TotalPrice { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { } // EF Core

    // ✅ Snapshot-based constructor (CORRECT)
    public Order(
        Guid orderId,
        Guid customerId,
        DateTime createdAt,
        decimal totalPrice)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId is required");

        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required");

        if (totalPrice <= 0)
            throw new ArgumentException("TotalPrice must be greater than zero");

        Id = orderId;
        CustomerId = customerId;
        CreatedAt = createdAt;
        TotalPrice = totalPrice;
        Status = OrderStatus.Pending;
    }

    // ❗ DO NOT touch TotalPrice here
    public void AddItem(
        Guid productId,
        string productName,
        decimal unitPrice,
        int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify order once processed");

        var item = new OrderItem(productId, productName, unitPrice, quantity);
        _items.Add(item);
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be paid");

        Status = OrderStatus.Paid;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Shipped)
            throw new InvalidOperationException("Shipped orders cannot be cancelled");

        Status = OrderStatus.Cancelled;
    }
}
