using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId)
               .IsRequired();

        builder.Property(o => o.Status)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(o => o.CreatedAt)
               .IsRequired();

        // ✅ FIX: Persisted snapshot value
        builder.Property(o => o.TotalPrice)
               .HasPrecision(18, 2)
               .IsRequired();

        // Backing field for items
        builder.Navigation(o => o.Items)
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Owned collection
        builder.OwnsMany(o => o.Items, oi =>
        {
            oi.ToTable("OrderItems");

            oi.WithOwner()
              .HasForeignKey("OrderId");

            oi.Property<Guid>("Id");
            oi.HasKey("Id");

            oi.Property(i => i.ProductId)
              .IsRequired();

            oi.Property(i => i.ProductName)
              .IsRequired();

            oi.Property(i => i.UnitPrice)
              .HasPrecision(18, 2)
              .IsRequired();

            oi.Property(i => i.Quantity)
              .IsRequired();
        });
    }
}
