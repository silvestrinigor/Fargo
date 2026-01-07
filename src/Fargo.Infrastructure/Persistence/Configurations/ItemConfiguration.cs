using Fargo.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder
                .HasOne(x => x.Article)
                .WithMany()
                .HasForeignKey(x => x.ArticleGuid)
                .IsRequired();
        }
    }
}
