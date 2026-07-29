using Fargo.Core.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.EntityTypeConfigurations;

public class ItemContainerConfiguration : IEntityTypeConfiguration<ItemContainer>
{
    public void Configure(EntityTypeBuilder<ItemContainer> builder)
    {
        builder.ToTable("item_containers");

        builder.HasKey(x => x.Guid);
    }
}
