using Fargo.Core.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class ItemContainerConfiguration : IEntityTypeConfiguration<ItemContainer>
{
    public void Configure(EntityTypeBuilder<ItemContainer> builder)
    {
        builder.ToTable("ItemContainers", t => t.IsTemporal());

        builder.HasKey(c => c.ItemGuid);

        builder.Property(c => c.ItemGuid)
            .ValueGeneratedNever();

        builder.HasOne(c => c.Item)
            .WithOne(i => i.Container)
            .HasForeignKey<ItemContainer>(c => c.ItemGuid)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
