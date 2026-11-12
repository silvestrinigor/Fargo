using Fargo.Application.Idempotency;
using Fargo.Application.IntegrationEvents;
using Fargo.Core.Audits;
using Fargo.Core.Barcodes;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.SupplyOrders;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Events;
using Fargo.Core.Identity.Authentication;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Identity.Tokens;
using Fargo.Core.Informations;
using Fargo.Infrastructure.Persistence.Configurations;
using Fargo.Infrastructure.Persistence.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Drawing;
using UnitsNet;

namespace Fargo.Infrastructure.Persistence;

public class FargoDbContext(DbContextOptions<FargoDbContext> options) : DbContext(options)
{
    public DbSet<Entity> Entities { get; set; }

    public DbSet<Event> Events { get; set; }

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

    public DbSet<SupplyOrder> SupplyOrders { get; set; }

    public DbSet<SupplyOrderLine> SupplyOrderLines { get; set; }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public DbSet<AuthorizationStamp> AuthorizationStamps { get; set; }

    public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }

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
        .ApplyConfiguration(new EventConfiguration())
        .ApplyConfiguration(new ArticleConfiguration())
        .ApplyConfiguration(new ArticleBarcodeConfiguration())
        .ApplyConfiguration(new ArticleDimensionConfiguration())
        .ApplyConfiguration(new ArticleVariationConfiguration())
        .ApplyConfiguration(new ArticlePackConfiguration())
        .ApplyConfiguration(new ArticleKitComponentConfiguration())
        .ApplyConfiguration(new ArticleContainerConfiguration())
        .ApplyConfiguration(new ArticleContainerAllowedArticleConfiguration())
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
        .ApplyConfiguration(new AuthenticationAttemptConfiguration())
        .ApplyConfiguration(new ServiceAccountConfiguration())
        .ApplyConfiguration(new ItemEventConfiguration())
        .ApplyConfiguration(new ItemMovimentEventConfiguration())
        .ApplyConfiguration(new ItemLifecycleEventConfiguration())
        .ApplyConfiguration(new SupplyOrderConfiguration())
        .ApplyConfiguration(new SupplyOrderLineConfiguration())
        .ApplyConfiguration(new SupplyOrderPartitionConfiguration())
        .ApplyConfiguration(new OutboxMessageConfiguration())
        .ApplyConfiguration(new AuthorizationStampConfiguration())
        .ApplyConfiguration(new IdempotencyRecordConfiguration());

        // Optimistic concurrency: map the PostgreSQL xmin system column as a
        // concurrency token (shadow property) on every mutable aggregate root
        // (those implementing IConcurrencyVersioned). xmin is maintained by
        // PostgreSQL and bumped on each row update, so a load-mutate-save that
        // races with a concurrent modification fails with a
        // DbUpdateConcurrencyException instead of silently overwriting it. No
        // schema column is added; the token requires no data migration.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // TPT roots (entities, events): EF Core applies a key's explicit
            // constraint name to *every* table of the hierarchy (one key is
            // shared by all of them, see dotnet/efcore#23444). The snake_case
            // naming convention names the root key while it is still alone
            // (e.g. "pk_entities"), so every derived table would get the same
            // primary key name, which PostgreSQL rejects (constraint/index
            // names are unique per schema). Removing the explicit name lets EF
            // derive a distinct default per table ("PK_<table>").
            if (entityType.BaseType is null
                && entityType.GetMappingStrategy() == RelationalAnnotationNames.TptMappingStrategy)
            {
                entityType.FindPrimaryKey()?.SetName(null);
            }

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
