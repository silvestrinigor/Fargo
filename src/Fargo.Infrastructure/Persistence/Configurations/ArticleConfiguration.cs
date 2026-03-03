using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            builder.ToTable(t => t.IsTemporal());

            builder.HasKey(x => x.Guid);

            builder
                .HasOne(x => x.UpdatedBy)
                .WithMany()
                .HasForeignKey(x => x.UpdatedByUserGuid);

            builder
                .Property(x => x.Name)
                .IsRequired();

            builder
                .Property(x => x.Description)
                .IsRequired();

            builder
                .HasMany(x => x.Partitions)
                .WithMany();
        }
    }
}