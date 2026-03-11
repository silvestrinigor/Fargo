using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    /// <summary>
    /// Configures the persistence mapping for the <see cref="UserGroup"/> entity.
    /// </summary>
    /// <remarks>
    /// This configuration defines keys, property mappings, relationships,
    /// and auditing metadata for <see cref="UserGroup"/>.
    ///
    /// Since <see cref="UserGroup"/> inherits from <see cref="AuditedEntity"/>,
    /// the inherited auditing properties are also configured here.
    /// </remarks>
    public sealed class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
    {
        /// <summary>
        /// Configures the database mapping for the <see cref="UserGroup"/> entity.
        /// </summary>
        /// <param name="builder">
        /// The builder used to configure the <see cref="UserGroup"/> entity type.
        /// </param>
        public void Configure(EntityTypeBuilder<UserGroup> builder)
        {
            builder.ToTable("UserGroups", tableBuilder => tableBuilder.IsTemporal());

            builder.HasKey(x => x.Guid);

            builder.HasAlternateKey(x => x.Nameid);

            builder.Property(x => x.Guid)
                .ValueGeneratedNever();

            builder.Property(x => x.Nameid)
                .IsRequired();

            builder.Property(x => x.Description)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.CreatedByGuid)
                .IsRequired();

            builder.Property(x => x.EditedAt)
                .IsRequired(false);

            builder.Property(x => x.EditedByGuid)
                .IsRequired(false);

            builder.HasMany(x => x.UserGroupPermissions)
                .WithOne(x => x.UserGroup)
                .HasForeignKey(x => x.UserGroupGuid)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.UserGroupPermissions)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}