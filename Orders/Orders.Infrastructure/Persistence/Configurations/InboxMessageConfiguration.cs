using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Infrastructure.Messaging;

namespace Orders.Infrastructure.Persistence.Configurations;

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessage"); // THIS FIXES EVERYTHING

        builder.HasKey(x => new { x.MessageId, x.Consumer });

        builder.Property(x => x.Consumer)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ProcessedAt)
            .IsRequired();
    }
}
