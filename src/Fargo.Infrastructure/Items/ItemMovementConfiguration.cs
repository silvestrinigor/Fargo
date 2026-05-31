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

        builder.Property(x => x.Guid).ValueGeneratedNever();

        builder.Property(x => x.FromParentContainerGuid).IsRequired(false);
        builder.Property(x => x.ToParentContainerGuid).IsRequired(false);

        builder.Ignore(x => x.ItemGuid);
        builder.Ignore(x => x.ActorGuid);
        builder.Ignore(x => x.OccurredAt);

        builder
            .HasOne(x => x.Event)
            .WithOne()
            .HasForeignKey<ItemMovement>(x => x.Guid)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.FromParentContainerGuid);
        builder.HasIndex(x => x.ToParentContainerGuid);
    }
}
