using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    /// <summary>
    /// Configures the database mapping for the <see cref="Article"/> entity.
    /// </summary>
    /// <remarks>
    /// This configuration defines how the <see cref="Article"/> entity is mapped
    /// to the database using Entity Framework Core.
    ///
    /// The entity is mapped to a temporal table, allowing automatic history
    /// tracking of changes over time.
    ///
    /// It also configures the primary key and the required properties used
    /// for persistence and auditing.
    /// </remarks>
    public class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        /// <summary>
        /// Configures the entity model for <see cref="Article"/>.
        /// </summary>
        /// <param name="builder">
        /// The builder used to configure the entity type.
        /// </param>
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            // Configure the table as a temporal table to enable historical tracking
            builder.ToTable(t => t.IsTemporal());

            // Configure the primary key
            builder.HasKey(x => x.Guid);

            // Configure required properties
            builder
                .Property(x => x.Name)
                .IsRequired();

            builder
                .Property(x => x.Description)
                .IsRequired();

            builder
                .Property(x => x.CreatedAt)
                .IsRequired();

            // Configure optional auditing properties
            builder
                .Property(x => x.CreatedByGuid);

            builder
                .Property(x => x.EditedAt);

            builder
                .Property(x => x.EditedByGuid);
        }
    }
}