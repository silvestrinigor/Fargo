using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    /// <summary>
    /// Configures the database mapping for the <see cref="PartitionAccess"/> entity.
    /// </summary>
    /// <remarks>
    /// This configuration defines how the <see cref="PartitionAccess"/> entity is mapped
    /// to the database using Entity Framework Core.
    ///
    /// The entity represents the access relationship between a <see cref="User"/>
    /// and a <see cref="Partition"/>.
    ///
    /// The table is configured as a temporal table, allowing historical tracking
    /// of access changes over time.
    ///
    /// It also defines the required foreign keys and the relationships with
    /// <see cref="User"/> and <see cref="Partition"/>.
    /// </remarks>
    public class PartitionAccessConfiguration : IEntityTypeConfiguration<PartitionAccess>
    {
        /// <summary>
        /// Configures the entity model for <see cref="PartitionAccess"/>.
        /// </summary>
        /// <param name="builder">
        /// The builder used to configure the entity type.
        /// </param>
        public void Configure(EntityTypeBuilder<PartitionAccess> builder)
        {
            // Configure the table as a temporal table to enable historical tracking
            builder.ToTable(t => t.IsTemporal());

            // Configure the primary key
            builder.HasKey(x => x.Guid);

            // Configure required foreign key properties
            builder
                .Property(x => x.UserGuid)
                .IsRequired();

            builder
                .Property(x => x.PartitionGuid)
                .IsRequired();

            // Configure relationship with User
            builder
                .HasOne(x => x.User)
                .WithMany(x => x.PartitionsAccesses)
                .HasForeignKey(x => x.UserGuid)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship with Partition
            builder
                .HasOne(x => x.Partition)
                .WithMany()
                .HasForeignKey(x => x.PartitionGuid)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}