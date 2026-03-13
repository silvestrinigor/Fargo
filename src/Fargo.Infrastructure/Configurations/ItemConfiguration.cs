using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    /// <summary>
    /// Configures the database mapping for the <see cref="Item"/> entity.
    /// </summary>
    /// <remarks>
    /// This configuration defines how the <see cref="Item"/> entity is mapped
    /// to the database using Entity Framework Core.
    ///
    /// The entity is mapped to a temporal table, enabling automatic history
    /// tracking of changes over time.
    ///
    /// It also configures the relationship between <see cref="Item"/> and
    /// <see cref="Article"/>, as well as auditing properties used for tracking
    /// creation and modification metadata.
    /// </remarks>
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        /// <summary>
        /// Configures the entity model for <see cref="Item"/>.
        /// </summary>
        /// <param name="builder">
        /// The builder used to configure the entity type.
        /// </param>
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            // Configure the table as a temporal table to enable historical tracking
            builder.ToTable(t => t.IsTemporal());

            // Configure the primary key
            builder.HasKey(x => x.Guid);

            // Configure the relationship with Article
            builder
                .HasOne(x => x.Article)
                .WithMany()
                .HasForeignKey(x => x.ArticleGuid)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Create an index for the foreign key
            builder.HasIndex(x => x.ArticleGuid);

            // Configure auditing properties
            builder
                .Property(x => x.CreatedAt)
                .IsRequired();

            builder
                .Property(x => x.CreatedByGuid);

            builder
                .Property(x => x.EditedAt);

            builder
                .Property(x => x.EditedByGuid);
        }
    }
}