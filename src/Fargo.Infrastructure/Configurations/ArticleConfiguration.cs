using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    public class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            builder.ToTable(t => t.IsTemporal());

            builder.HasKey(x => x.Guid);

            builder.Property(x => x.Name).IsRequired();

            builder.Property(x => x.Description).IsRequired();

            builder.Property(x => x.CreatedAt).IsRequired();

            builder.Property(x => x.CreatedByGuid);

            builder.Property(x => x.EditedAt);

            builder.Property(x => x.EditedByGuid);

            builder.HasMany(a => a.Partitions).WithMany(p => p.ArticleMembers);
        }
    }
}