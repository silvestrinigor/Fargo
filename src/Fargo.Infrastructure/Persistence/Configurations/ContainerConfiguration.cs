using Fargo.Domain.Entities.ArticleItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class ContainerConfiguration : IEntityTypeConfiguration<ItemClosure>
    {
        public void Configure(EntityTypeBuilder<ItemClosure> builder)
        {
            builder
                .HasKey(x => x.AncestorItemGuid);

            builder
                .HasOne(x => x.AncestorItem)
                .WithOne(x => x.ContainerExtension)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
