using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    /// <summary>
    /// Configures the database mapping for the <see cref="RefreshToken"/> entity.
    /// </summary>
    /// <remarks>
    /// This configuration defines how the <see cref="RefreshToken"/> entity is mapped
    /// to the database using Entity Framework Core.
    ///
    /// The entity is stored in a temporal table, allowing the system to keep
    /// historical records of refresh token changes.
    ///
    /// A unique index is created for the token hash to guarantee that each
    /// stored refresh token hash is unique in the database.
    ///
    /// The configuration also defines the relationship between refresh tokens
    /// and users, ensuring that tokens are deleted automatically when the
    /// associated user is removed.
    /// </remarks>
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        /// <summary>
        /// Configures the entity model for <see cref="RefreshToken"/>.
        /// </summary>
        /// <param name="builder">
        /// The builder used to configure the entity type.
        /// </param>
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            // Configure the table as a temporal table to enable historical tracking
            builder.ToTable(t => t.IsTemporal());

            // Configure the primary key
            builder.HasKey(x => x.Guid);

            // Configure unique index for the token hash
            builder.HasIndex(x => x.TokenHash).IsUnique();

            // Configure relationship with User
            builder
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserGuid)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure optional token replacement reference
            builder.Property(x => x.ReplacedByTokenHash);

            // Configure expiration timestamp
            builder.Property(x => x.ExpiresAt);
        }
    }
}