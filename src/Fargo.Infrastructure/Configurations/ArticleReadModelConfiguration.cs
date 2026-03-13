using Fargo.Application.Models.ArticleModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    /// <summary>
    /// Configures the database mapping for the <see cref="ArticleReadModel"/>.
    /// </summary>
    /// <remarks>
    /// This configuration defines how the <see cref="ArticleReadModel"/> is mapped
    /// to the database using Entity Framework Core.
    ///
    /// The model is mapped to a temporal table, allowing historical queries
    /// over previous versions of the data.
    ///
    /// This read model is typically used for query operations where a simplified
    /// representation of the article entity is required.
    /// </remarks>
    public sealed class ArticleReadModelConfiguration : IEntityTypeConfiguration<ArticleReadModel>
    {
        /// <summary>
        /// Configures the entity model for <see cref="ArticleReadModel"/>.
        /// </summary>
        /// <param name="builder">
        /// The builder used to configure the entity type.
        /// </param>
        public void Configure(EntityTypeBuilder<ArticleReadModel> builder)
        {
            // Configure the table as a temporal table to enable historical tracking
            builder.ToTable(t => t.IsTemporal());

            // Configure the primary key
            builder.HasKey(x => x.Guid);
        }
    }
}