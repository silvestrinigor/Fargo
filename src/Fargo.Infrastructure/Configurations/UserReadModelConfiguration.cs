using Fargo.Application.Models.UserModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    /// <summary>
    /// Configures the database mapping for the <see cref="UserReadModel"/>.
    /// </summary>
    /// <remarks>
    /// This configuration defines how the <see cref="UserReadModel"/> is mapped
    /// to the database using Entity Framework Core.
    ///
    /// The model is stored in a temporal table, enabling historical queries
    /// over previous versions of user data.
    ///
    /// An alternate key is defined for <c>Nameid</c> to guarantee that each
    /// username is unique.
    ///
    /// The configuration also defines relationships with user permissions and
    /// user groups, as well as a value conversion for the password expiration
    /// period stored as ticks in the database.
    /// </remarks>
    public class UserReadModelConfiguration : IEntityTypeConfiguration<UserReadModel>
    {
        /// <summary>
        /// Configures the entity model for <see cref="UserReadModel"/>.
        /// </summary>
        /// <param name="builder">
        /// The builder used to configure the entity type.
        /// </param>
        public void Configure(EntityTypeBuilder<UserReadModel> builder)
        {
            // Configure the table as a temporal table to enable historical tracking
            builder.ToTable(t => t.IsTemporal());

            // Configure the primary key
            builder.HasKey(x => x.Guid);

            // Configure alternate key to enforce unique Nameid
            builder.HasAlternateKey(x => x.Nameid);

            // Configure relationship with user permissions
            builder
                .HasMany(x => x.UserPermissions)
                .WithOne()
                .HasForeignKey(x => x.UserGuid);

            // Configure conversion for TimeSpan stored as ticks
            builder
                .Property(x => x.DefaultPasswordExpirationPeriod)
                .HasConversion(
                        x => x.Ticks,
                        x => TimeSpan.FromTicks(x)
                        );

            // Configure many-to-many relationship with user groups
            builder
                .HasMany(x => x.UserGroups)
                .WithMany();
        }
    }
}