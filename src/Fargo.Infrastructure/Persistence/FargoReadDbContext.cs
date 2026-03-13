using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Models.UserGroupModels;
using Fargo.Application.Models.UserModels;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Configurations;
using Fargo.Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence
{
    /// <summary>
    /// Represents the read-side database context for the Fargo application.
    /// </summary>
    /// <remarks>
    /// This context is responsible for persisting and querying read models used
    /// by the query side of the application (CQRS pattern).
    ///
    /// It exposes DbSet properties for all read models and applies the required
    /// entity configurations and value object conversions.
    ///
    /// The context also configures conventions for commonly used value objects,
    /// ensuring consistent database column length and conversion behavior across
    /// all entities that use them.
    /// </remarks>
    public class FargoReadDbContext(DbContextOptions<FargoReadDbContext> options) : DbContext(options)
    {
        /// <summary>
        /// Gets the set of article read models.
        /// </summary>
        public DbSet<ArticleReadModel> Articles { get; set; }

        /// <summary>
        /// Gets the set of item read models.
        /// </summary>
        public DbSet<ItemReadModel> Items { get; set; }

        /// <summary>
        /// Gets the set of user read models.
        /// </summary>
        public DbSet<UserReadModel> Users { get; set; }

        /// <summary>
        /// Gets the set of user permission read models.
        /// </summary>
        public DbSet<UserPermissionReadModel> UserPermission { get; set; }

        /// <summary>
        /// Gets the set of user group read models.
        /// </summary>
        public DbSet<UserGroupReadModel> UserGroups { get; set; }

        /// <summary>
        /// Gets the set of user group permission read models.
        /// </summary>
        public DbSet<UserGroupPermissionReadModel> UserGroupPermissions { get; set; }

        /// <summary>
        /// Configures global conventions for value object mappings.
        /// </summary>
        /// <remarks>
        /// This method defines consistent conversion and column length settings
        /// for domain value objects used throughout the read models.
        ///
        /// Each value object is mapped to its string representation in the database
        /// using the corresponding converter.
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
                .Properties<FirstName>()
                .HaveMaxLength(FirstName.MaxLength)
                .HaveConversion<FirstNameStringConverter>();

            configurationBuilder
                .Properties<LastName>()
                .HaveMaxLength(LastName.MaxLength)
                .HaveConversion<LastNameStringConverter>();
        }

        /// <summary>
        /// Applies entity configurations for all read models.
        /// </summary>
        /// <param name="modelBuilder">
        /// The builder used to construct the entity model.
        /// </param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ArticleReadModelConfiguration());

            modelBuilder.ApplyConfiguration(new ItemReadModelConfiguration());

            modelBuilder.ApplyConfiguration(new UserReadModelConfiguration());

            modelBuilder.ApplyConfiguration(new UserPermissionReadModelConfiguration());

            modelBuilder.ApplyConfiguration(new UserGroupReadModelConfiguration());

            modelBuilder.ApplyConfiguration(new UserGroupPermissionReadModelConfiguration());
        }
    }
}