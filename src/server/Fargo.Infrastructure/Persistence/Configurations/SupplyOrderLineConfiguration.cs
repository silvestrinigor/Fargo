using Fargo.Core.Entities.SupplyOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public sealed class SupplyOrderLineConfiguration : IEntityTypeConfiguration<SupplyOrderLine>
{
    public void Configure(EntityTypeBuilder<SupplyOrderLine> builder)
    {
        builder.ToTable("supply_order_lines");

        // One line per article on a given order.
        builder.HasKey(x => new
        {
            x.SupplyOrderGuid,
            x.ArticleGuid
        });

        builder.Property(x => x.OrderedQuantity);

        builder.Property(x => x.ReceivedQuantity);

        // The owning order relationship (inverse side) is declared on the
        // SupplyOrder configuration via HasMany(x => x.Lines).

        builder
        .HasOne(x => x.Article)
        .WithMany()
        .HasForeignKey(x => x.ArticleGuid)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
