using Fargo.Application.Models.UserGroupModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    /// <summary>
    /// Configures the database mapping for the <see cref="UserGroupPermissionReadModel"/>.
    /// </summary>
    /// <remarks>
    /// This configuration defines how the <see cref="UserGroupPermissionReadModel"/>
    /// is mapped to the database using Entity Framework Core.
    ///
    /// The model is stored in a temporal table, allowing queries against historical
    /// versions of permission assignments.
    ///
    /// An alternate key is defined using the combination of <c>UserGroupGuid</c>
    /// and <c>Action</c>, ensuring that each user group can only have one entry
    /// for a specific permission action.
    /// </remarks>
    public class UserGroupPermissionReadModelConfiguration
        : IEntityTypeConfiguration<UserGroupPermissionReadModel>
    {
        /// <summary>
        /// Configures the entity model for <see cref="UserGroupPermissionReadModel"/>.
        /// </summary>
        /// <param name="builder">
        /// The builder used to configure the entity type.
        /// </param>
        public void Configure(EntityTypeBuilder<UserGroupPermissionReadModel> builder)
        {
            // Configure the table as a temporal table to enable historical tracking
            builder.ToTable(t => t.IsTemporal());

            // Configure the primary key
            builder.HasKey(x => x.Guid);

            // Configure alternate key to ensure uniqueness of permissions per user group
            builder.HasAlternateKey(x => new { x.UserGroupGuid, x.Action });
        }
    }
}