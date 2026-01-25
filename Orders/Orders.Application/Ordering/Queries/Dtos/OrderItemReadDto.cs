//namespace Orders.Application.Ordering.Queries.Dtos;

//public class OrderItemReadDto
//{
//    public Guid ProductId { get; set; }
//    public string ProductName { get; set; } = default!;
//    public decimal UnitPrice { get; set; }
//    public int Quantity { get; set; }
//}

namespace Orders.Application.Ordering.Queries.Dtos;

public class OrderItemReadDto
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public decimal UnitPrice { get; }
    public int Quantity { get; }

    public OrderItemReadDto(
        Guid productId,
        string productName,
        decimal unitPrice,
        int quantity)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}
