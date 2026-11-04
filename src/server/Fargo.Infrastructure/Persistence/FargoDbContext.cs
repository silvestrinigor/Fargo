using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Barcodes;
using Fargo.Core.Entities;
using Fargo.Core.Identity;
using Fargo.Core.Informations;
using Fargo.Core.Entities.Items;
using Fargo.Core.Events;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Security;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Infrastructure.Configurations;
using Fargo.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using UnitsNet;

namespace Fargo.Infrastructure.Persistence;

public class FargoDbContext(DbContextOptions<FargoDbContext> options) : DbContext(options)
{
    public DbSet<Entity> Entities { get; set; }

    public DbSet<Article> Articles { get; set; }

    public DbSet<Item> Items { get; set; }

    public DbSet<ItemEvent> ItemEvents { get; set; }

    public DbSet<ItemMovimentEvent> ItemMovimentEvents { get; set; }

    public DbSet<ItemLifecycleEvent> ItemLifecycleEvents { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Actor> Actors { get; set; }

    public DbSet<UserGroup> UserGroups { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Partition> Partitions { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

    public DbSet<AuthenticationAttempt> AuthenticationAttempts { get; set; }

    public DbSet<ServiceAccount> ServiceAccounts { get; set; }

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

        configurationBuilder
        .Properties<Mass>()
        .HaveConversion<MassJsonConverter>()
        .HaveColumnType("jsonb");

        configurationBuilder
        .Properties<Length>()
        .HaveConversion<LengthJsonConverter>()
        .HaveColumnType("jsonb");

        configurationBuilder
        .Properties<Scalar>()
        .HaveConversion<ScalarDoubleConverter>();

        configurationBuilder
        .Properties<Color>()
        .HaveConversion<ColorArgbConverter>();

        configurationBuilder
        .Properties<TimeSpan>()
        .HaveConversion<TimeSpanTicksConverter>();

        configurationBuilder
        .Properties<Ean13>()
        .HaveMaxLength(Ean13.CodeLength)
        .HaveConversion<Ean13StringConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
        .ApplyConfiguration(new EntityConfiguration())
        .ApplyConfiguration(new ArticleConfiguration())
        .ApplyConfiguration(new ArticleBarcodeConfiguration())
        .ApplyConfiguration(new ArticleDimensionConfiguration())
        .ApplyConfiguration(new ArticleVariationConfiguration())
        .ApplyConfiguration(new ArticlePackConfiguration())
        .ApplyConfiguration(new ArticleKitComponentConfiguration())
        .ApplyConfiguration(new ArticleContainerConfiguration())
        .ApplyConfiguration(new ArticlePartitionConfiguration())
        .ApplyConfiguration(new ItemConfiguration())
        .ApplyConfiguration(new ItemMovimentConfiguration())
        .ApplyConfiguration(new ItemPartitionConfiguration())
        .ApplyConfiguration(new ItemParentContainerHistoryPostgresTemporalConfiguration())
        .ApplyConfiguration(new ActorConfiguration())
        .ApplyConfiguration(new ActorPartitionConfiguration())
        .ApplyConfiguration(new ActorPartitionAccessConfiguration())
        .ApplyConfiguration(new UserConfiguration())
        .ApplyConfiguration(new UserAuthenticationConfiguration())
        .ApplyConfiguration(new UserUserGroupConfiguration())
        .ApplyConfiguration(new UserGroupConfiguration())
        .ApplyConfiguration(new UserGroupPartitionConfiguration())
        .ApplyConfiguration(new UserGroupPartitionAccessConfiguration())
        .ApplyConfiguration(new RefreshTokenConfiguration())
        .ApplyConfiguration(new PartitionConfiguration())
        .ApplyConfiguration(new AuditLogConfiguration())
        .ApplyConfiguration(new AuditLogPartitionConfiguration())
        .ApplyConfiguration(new AuthenticationAttemptConfiguration())
        .ApplyConfiguration(new ServiceAccountConfiguration())
        .ApplyConfiguration(new ItemEventConfiguration())
        .ApplyConfiguration(new ItemMovimentEventConfiguration())
        .ApplyConfiguration(new ItemLifecycleEventConfiguration());

        // Optimistic concurrency: map the PostgreSQL xmin system column as a
        // concurrency token (shadow property) on every mutable aggregate root
        // (those implementing IConcurrencyVersioned). xmin is maintained by
        // PostgreSQL and bumped on each row update, so a load-mutate-save that
        // races with a concurrent modification fails with a
        // DbUpdateConcurrencyException instead of silently overwriting it. No
        // schema column is added; the token requires no data migration.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IConcurrencyVersioned).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder
                .Entity(entityType.ClrType)
                .Property<uint>("xmin")
                .HasColumnName("xmin")
                .IsRowVersion();
            }
        }
    }
}
