using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    /// <summary>
    /// Configures the persistence mapping for the <see cref="User"/> entity.
    /// </summary>
    /// <remarks>
    /// This configuration defines keys, property mappings, relationships,
    /// conversions, and auditing metadata for <see cref="User"/>.
    ///
    /// Since <see cref="User"/> inherits from <see cref="AuditedEntity"/>,
    /// the inherited auditing properties are also configured here.
    /// </remarks>
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        /// <summary>
        /// Configures the database mapping for the <see cref="User"/> entity.
        /// </summary>
        /// <param name="builder">
        /// The builder used to configure the <see cref="User"/> entity type.
        /// </param>
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users", tableBuilder => tableBuilder.IsTemporal());

            builder.HasKey(x => x.Guid);

            builder.HasAlternateKey(x => x.Nameid);

            builder.Property(x => x.Guid)
                .ValueGeneratedNever();

            builder.Property(x => x.Nameid)
                .IsRequired();

            builder.Property(x => x.FirstName)
                .IsRequired(false);

            builder.Property(x => x.LastName)
                .IsRequired(false);

            builder.Property(x => x.Description)
                .IsRequired();

            builder.Property(x => x.PasswordHash)
                .IsRequired();

            builder.Property(x => x.DefaultPasswordExpirationPeriod)
                .HasConversion(
                    x => x.Ticks,
                    x => TimeSpan.FromTicks(x))
                .IsRequired();

            builder.Property(x => x.RequirePasswordChangeAt)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.CreatedByGuid)
                .IsRequired(false);

            builder.Property(x => x.EditedAt)
                .IsRequired(false);

            builder.Property(x => x.EditedByGuid)
                .IsRequired(false);

            builder.HasMany(x => x.UserPermissions)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserGuid)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.UserPermissions)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}