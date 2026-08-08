using Fargo.Core.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class ArticlePartitionConfiguration : IEntityTypeConfiguration<ArticlePartition>
{
    public void Configure(EntityTypeBuilder<ArticlePartition> builder)
    {
        builder.ToTable("article_partitions");

        builder.HasKey(x => new
        {
            x.ArticleGuid,
            x.PartitionGuid
        });

        builder
        .HasOne(x => x.Article)
        .WithMany(x => x.Partitions)
        .HasForeignKey(x => x.ArticleGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(x => x.Partition)
        .WithMany()
        .HasForeignKey(x => x.PartitionGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
