using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fargo.Core.Entities;

namespace Fargo.Infrastructure.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder
            .HasKey(a => a.Guid);
        builder
            .Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);
        builder
            .Property(a => a.CreatedAt)
            .IsRequired();
    }
}
