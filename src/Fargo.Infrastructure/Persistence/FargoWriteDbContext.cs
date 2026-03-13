using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Configurations;
using Fargo.Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence
{
    /// <summary>
    /// Represents the write-side database context for the Fargo application.
    /// </summary>
    /// <remarks>
    /// This context is responsible for persisting domain entities used by the
    /// command side of the application (CQRS pattern).
    ///
    /// It exposes DbSet properties for all aggregate roots and entities that
    /// participate in domain operations.
    ///
    /// The context also defines global conventions for domain value objects,
    /// ensuring consistent column length and conversion behavior when mapping
    /// them to the database.
    /// </remarks>
    public class FargoWriteDbContext(DbContextOptions<FargoWriteDbContext> options) : DbContext(options)
    {
        /// <summary>
        /// Gets the set of <see cref="Article"/> entities.
        /// </summary>
        public DbSet<Article> Articles { get; set; }

        /// <summary>
        /// Gets the set of <see cref="Item"/> entities.
        /// </summary>
        public DbSet<Item> Items { get; set; }

        /// <summary>
        /// Gets the set of <see cref="User"/> entities.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets the set of <see cref="UserPermission"/> entities.
        /// </summary>
        public DbSet<UserPermission> UserPermission { get; set; }

        /// <summary>
        /// Gets the set of <see cref="UserGroup"/> entities.
        /// </summary>
        public DbSet<UserGroup> UserGroups { get; set; }

        /// <summary>
        /// Gets the set of <see cref="UserGroupPermission"/> entities.
        /// </summary>
        public DbSet<UserGroupPermission> UserGroupPermissions { get; set; }

        /// <summary>
        /// Gets the set of <see cref="RefreshToken"/> entities.
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// Gets the set of <see cref="Partition"/> entities.
        /// </summary>
        public DbSet<Partition> Partitions { get; set; }

        /// <summary>
        /// Gets the set of <see cref="PartitionAccess"/> entities.
        /// </summary>
        public DbSet<PartitionAccess> PartitionAccesses { get; set; }

        /// <summary>
        /// Configures global conventions for domain value objects.
        /// </summary>
        /// <remarks>
        /// Each value object is mapped to its string representation in the
        /// database using the appropriate converter and maximum length.
        ///
        /// This ensures consistent mapping behavior across all entities
        /// that use these value objects.
        /// </remarks>
        /// <param name="configurationBuilder">
        /// The builder used to configure model conventions.
        /// </param>
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<Name>()
                .HaveMaxLength(Name.MaxLength)
                .HaveConversion<NameStringConverter>();

            configurationBuilder
                .Properties<Description>()
                .HaveMaxLength(Description.MaxLength)
                .HaveConversion<DescriptionStringConverter>();

            configurationBuilder
                .Properties<Nameid>()
                .HaveMaxLength(Nameid.MaxLength)
                .HaveConversion<NameidStringConverter>();

            configurationBuilder
                .Properties<PasswordHash>()
                .HaveMaxLength(PasswordHash.MaxLength)
                .HaveConversion<PasswordHashStringConverter>();

            configurationBuilder
                .Properties<TokenHash>()
                .HaveMaxLength(TokenHash.MaxLength)
                .HaveConversion<TokenHashStringConverter>();

            configurationBuilder
                .Properties<FirstName>()
                .HaveMaxLength(FirstName.MaxLength)
                .HaveConversion<FirstNameStringConverter>();

            configurationBuilder
                .Properties<LastName>()
                .HaveMaxLength(LastName.MaxLength)
                .HaveConversion<LastNameStringConverter>();
        }

        /// <summary>
        /// Applies entity configurations for all domain entities.
        /// </summary>
        /// <param name="modelBuilder">
        /// The builder used to construct the entity model.
        /// </param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ArticleConfiguration());

            modelBuilder.ApplyConfiguration(new ItemConfiguration());

            modelBuilder.ApplyConfiguration(new UserConfiguration());

            modelBuilder.ApplyConfiguration(new UserPermissionConfiguration());

            modelBuilder.ApplyConfiguration(new UserGroupConfiguration());

            modelBuilder.ApplyConfiguration(new UserGroupPermissionConfiguration());

            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

            modelBuilder.ApplyConfiguration(new PartitionConfiguration());

            modelBuilder.ApplyConfiguration(new PartitionAccessConfiguration());
        }
    }
}