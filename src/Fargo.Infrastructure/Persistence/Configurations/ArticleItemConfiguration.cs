using Fargo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class ArticleItemConfiguration : IEntityTypeConfiguration<ArticleItem>
    {
        public void Configure(EntityTypeBuilder<ArticleItem> builder)
        {
        }
    }
}
