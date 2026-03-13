using Fargo.Application.Models.UserGroupModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    /// <summary>
    /// Configures the database mapping for the <see cref="UserGroupReadModel"/>.
    /// </summary>
    /// <remarks>
    /// This configuration defines how the <see cref="UserGroupReadModel"/> is mapped
    /// to the database using Entity Framework Core.
    ///
    /// The model is stored in a temporal table, enabling historical queries over
    /// previous versions of user group data.
    ///
    /// An alternate key is configured for <c>Nameid</c> to guarantee that each
    /// user group name identifier is unique.
    ///
    /// A relationship is also defined between the user group and its associated
    /// permission read models.
    /// </remarks>
    public class UserGroupReadModelConfiguration : IEntityTypeConfiguration<UserGroupReadModel>
    {
        /// <summary>
        /// Configures the entity model for <see cref="UserGroupReadModel"/>.
        /// </summary>
        /// <param name="builder">
        /// The builder used to configure the entity type.
        /// </param>
        public void Configure(EntityTypeBuilder<UserGroupReadModel> builder)
        {
            // Configure the table as a temporal table to enable historical tracking
            builder.ToTable(t => t.IsTemporal());

            // Configure the primary key
            builder.HasKey(x => x.Guid);

            // Configure alternate key to enforce unique Nameid
            builder.HasAlternateKey(x => x.Nameid);

            // Configure relationship with UserGroupPermissionReadModel
            builder
                .HasMany(x => x.UserGroupPermissions)
                .WithOne()
                .HasForeignKey(x => x.UserGroupGuid);
        }
    }
}