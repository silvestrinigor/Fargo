using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    /// <summary>
    /// Configures the database mapping for the <see cref="UserPermission"/> entity.
    /// </summary>
    /// <remarks>
    /// This configuration defines how the <see cref="UserPermission"/> entity is mapped
    /// to the database using Entity Framework Core.
    ///
    /// The entity is stored in a temporal table, enabling automatic tracking of
    /// historical changes to user permissions.
    ///
    /// An alternate key is configured using the combination of <c>UserGuid</c> and
    /// <c>Action</c>, ensuring that a user cannot have duplicate entries for the
    /// same permission action.
    /// </remarks>
    public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
    {
        /// <summary>
        /// Configures the entity model for <see cref="UserPermission"/>.
        /// </summary>
        /// <param name="builder">
        /// The builder used to configure the entity type.
        /// </param>
        public void Configure(EntityTypeBuilder<UserPermission> builder)
        {
            // Configure the table as a temporal table to enable historical tracking
            builder.ToTable(t => t.IsTemporal());

            // Configure the primary key
            builder.HasKey(x => x.Guid);

            // Configure alternate key to enforce unique permission per user
            builder.HasAlternateKey(x => new { x.UserGuid, x.Action });
        }
    }
}