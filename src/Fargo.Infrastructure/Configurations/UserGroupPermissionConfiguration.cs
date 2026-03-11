using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    /// <summary>
    /// Configures the persistence mapping for the <see cref="UserGroupPermission"/> entity.
    /// </summary>
    /// <remarks>
    /// This configuration defines keys, property mappings, and relationship
    /// settings for permissions assigned to a <see cref="UserGroup"/>.
    /// </remarks>
    public sealed class UserGroupPermissionConfiguration : IEntityTypeConfiguration<UserGroupPermission>
    {
        /// <summary>
        /// Configures the database mapping for the <see cref="UserGroupPermission"/> entity.
        /// </summary>
        /// <param name="builder">
        /// The builder used to configure the <see cref="UserGroupPermission"/> entity type.
        /// </param>
        public void Configure(EntityTypeBuilder<UserGroupPermission> builder)
        {
            builder.ToTable("UserGroupPermissions", tableBuilder => tableBuilder.IsTemporal());

            builder.HasKey(x => x.Guid);

            builder.HasAlternateKey(x => new
            {
                x.UserGroupGuid,
                x.Action
            });

            builder.Property(x => x.Guid)
                .ValueGeneratedNever();

            builder.Property(x => x.UserGroupGuid)
                .IsRequired();

            builder.Property(x => x.Action)
                .IsRequired();

            builder.HasOne(x => x.UserGroup)
                .WithMany(x => x.UserGroupPermissions)
                .HasForeignKey(x => x.UserGroupGuid)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}