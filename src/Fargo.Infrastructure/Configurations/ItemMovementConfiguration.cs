using Fargo.Core.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class ItemMovementConfiguration : IEntityTypeConfiguration<ItemMovement>
{
    public void Configure(EntityTypeBuilder<ItemMovement> builder)
    {
        builder.ToTable("ItemMovements");

        builder.HasKey(x => x.Guid);

        builder.Property(x => x.ItemGuid).IsRequired();
        builder.Property(x => x.FromParentContainerGuid).IsRequired(false);
        builder.Property(x => x.ToParentContainerGuid).IsRequired(false);
        builder.Property(x => x.ActorGuid).IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();

        builder.HasIndex(x => new { x.ItemGuid, x.OccurredAt });
        builder.HasIndex(x => x.ActorGuid);
    }
}
