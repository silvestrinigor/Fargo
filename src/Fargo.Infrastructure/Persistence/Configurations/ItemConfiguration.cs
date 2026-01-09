using Fargo.Domain.Entities.ArticleItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder
                .ToTable(t => t.IsTemporal());

            builder
                .HasKey(x => x.Guid);

            builder
                .HasOne(x => x.Article)
                .WithMany()
                .HasForeignKey(x => x.ArticleGuid)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.ContainerExtension)
                .WithOne(x => x.AncestorItem)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.ParentItem);

            builder
                .HasIndex(x => x.ArticleGuid);
        }
    }
}
