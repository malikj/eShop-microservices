using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }

    public int CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Required by EF Core
    private Product() { }

    // Domain constructor
    public Product(string name, string description, decimal price, int categoryId)
    {
        Id = Guid.NewGuid();          // ✅ IMPORTANT
        Name = name;
        Description = description;
        Price = price;
        CategoryId = categoryId;
        IsActive = true;
    }
}
