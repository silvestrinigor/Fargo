using Fargo.Core.Entities.SupplyOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public sealed class SupplyOrderConfiguration : IEntityTypeConfiguration<SupplyOrder>
{
    public void Configure(EntityTypeBuilder<SupplyOrder> builder)
    {
        builder.ToTable("supply_orders");

        builder.Property(x => x.Reference);

        builder.Property(x => x.Description);

        builder.Property(x => x.Status);

        builder.Property(x => x.CreatedAt);

        builder.Property(x => x.PlacedAt);

        builder.Property(x => x.ReceivingContainerGuid);

        builder.HasIndex(x => x.Status);

        // The receiving container is an item (whose article is a container).
        // Restrict deletion so removing a container surfaces the orders still
        // pointing at it rather than silently blanking their receiving location.
        builder
        .HasOne(x => x.ReceivingContainer)
        .WithMany()
        .HasForeignKey(x => x.ReceivingContainerGuid)
        .OnDelete(DeleteBehavior.Restrict);

        builder
        .HasMany(x => x.Lines)
        .WithOne(x => x.SupplyOrder)
        .HasForeignKey(x => x.SupplyOrderGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
