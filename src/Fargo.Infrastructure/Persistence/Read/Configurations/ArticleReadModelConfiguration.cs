using Fargo.Application.Models.ArticleModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Read.Configurations
{
    public sealed class ArticleReadModelConfiguration : IEntityTypeConfiguration<ArticleReadModel>
    {
        public void Configure(EntityTypeBuilder<ArticleReadModel> builder)
        {
            builder
                .ToTable(t => t.IsTemporal());

            builder
                .HasKey(x => x.Guid);
        }
    }
}
