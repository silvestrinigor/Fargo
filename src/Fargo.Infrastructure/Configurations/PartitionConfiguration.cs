using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    /// <summary>
    /// Configures the database mapping for the <see cref="Partition"/> entity.
    /// </summary>
    /// <remarks>
    /// This configuration defines how the <see cref="Partition"/> entity is mapped
    /// to the database using Entity Framework Core.
    ///
    /// The entity is stored in a temporal table, enabling automatic tracking
    /// of historical changes over time.
    ///
    /// It also defines the self-referencing relationship that allows a partition
    /// to have a parent partition, creating a hierarchical structure.
    ///
    /// Additional properties are configured to support partition behavior flags
    /// and auditing metadata.
    /// </remarks>
    public class PartitionConfiguration : IEntityTypeConfiguration<Partition>
    {
        /// <summary>
        /// Configures the entity model for <see cref="Partition"/>.
        /// </summary>
        /// <param name="builder">
        /// The builder used to configure the entity type.
        /// </param>
        public void Configure(EntityTypeBuilder<Partition> builder)
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
                .Property(x => x.IsActive)
                .IsRequired();

            builder
                .Property(x => x.IsGlobal)
                .IsRequired();

            builder
                .Property(x => x.IsEditable)
                .IsRequired();

            // Configure optional parent partition reference
            builder
                .Property(x => x.ParentPartitionGuid);

            builder
                .HasOne(x => x.ParentPartition)
                .WithMany()
                .HasForeignKey(x => x.ParentPartitionGuid)
                .OnDelete(DeleteBehavior.Restrict);

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