using Fargo.Application.Models.ItemModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    /// <summary>
    /// Configures the database mapping for the <see cref="ItemReadModel"/>.
    /// </summary>
    /// <remarks>
    /// This configuration defines how the <see cref="ItemReadModel"/> is mapped
    /// to the database using Entity Framework Core.
    ///
    /// The read model is mapped to a temporal table, allowing queries against
    /// historical versions of the data.
    ///
    /// This model is typically used for read/query operations where a simplified
    /// representation of the <see cref="Item"/> entity is required.
    /// </remarks>
    public class ItemReadModelConfiguration : IEntityTypeConfiguration<ItemReadModel>
    {
        /// <summary>
        /// Configures the entity model for <see cref="ItemReadModel"/>.
        /// </summary>
        /// <param name="builder">
        /// The builder used to configure the entity type.
        /// </param>
        public void Configure(EntityTypeBuilder<ItemReadModel> builder)
        {
            // Configure the table as a temporal table to enable historical tracking
            builder.ToTable(x => x.IsTemporal());

            // Configure the primary key
            builder.HasKey(x => x.Guid);
        }
    }
}