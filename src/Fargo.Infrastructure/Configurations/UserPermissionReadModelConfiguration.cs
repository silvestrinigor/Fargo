using Fargo.Application.Models.UserModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    /// <summary>
    /// Configures the database mapping for the <see cref="UserPermissionReadModel"/>.
    /// </summary>
    /// <remarks>
    /// This configuration defines how the <see cref="UserPermissionReadModel"/> is mapped
    /// to the database using Entity Framework Core.
    ///
    /// The model is stored in a temporal table, allowing historical queries over
    /// previous versions of user permission data.
    ///
    /// An alternate key is configured using the combination of <c>UserGuid</c> and
    /// <c>Action</c>, ensuring that each user has at most one entry for a given
    /// permission action.
    /// </remarks>
    public class UserPermissionReadModelConfiguration : IEntityTypeConfiguration<UserPermissionReadModel>
    {
        /// <summary>
        /// Configures the entity model for <see cref="UserPermissionReadModel"/>.
        /// </summary>
        /// <param name="builder">
        /// The builder used to configure the entity type.
        /// </param>
        public void Configure(EntityTypeBuilder<UserPermissionReadModel> builder)
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