using Fargo.Core.Articles;
using Fargo.Core.Audits;
using Fargo.Core.Barcodes;
using Fargo.Core.Identity;
using Fargo.Core.Informations;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.Security;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Fargo.Infrastructure.Configurations;
using Fargo.Infrastructure.Entities;
using Fargo.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using UnitsNet;

namespace Fargo.Infrastructure.Persistence;

public class FargoDbContext(DbContextOptions<FargoDbContext> options) : DbContext(options)
{
    public DbSet<Article> Articles { get; set; }

    public DbSet<Item> Items { get; set; }

    public DbSet<ItemParentContainerHistoryPostgresTemporal> ItemParentContainerHistories { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<UserGroup> UserGroups { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Partition> Partitions { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

    public DbSet<AuthenticationAttempt> AuthenticationAttempts { get; set; }

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
        .HaveConversion<MassStringConverter>();

        configurationBuilder
        .Properties<Length>()
        .HaveConversion<LengthStringConverter>();

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
        .ApplyConfiguration(new UserConfiguration())
        .ApplyConfiguration(new UserAuthenticationConfiguration())
        .ApplyConfiguration(new UserUserGroupConfiguration())
        .ApplyConfiguration(new UserPartitionConfiguration())
        .ApplyConfiguration(new UserPartitionAccessConfiguration())
        .ApplyConfiguration(new UserGroupConfiguration())
        .ApplyConfiguration(new UserGroupPartitionConfiguration())
        .ApplyConfiguration(new UserGroupPartitionAccessConfiguration())
        .ApplyConfiguration(new RefreshTokenConfiguration())
        .ApplyConfiguration(new PartitionConfiguration())
        .ApplyConfiguration(new AuditLogConfiguration())
        .ApplyConfiguration(new AuditLogPartitionConfiguration())
        .ApplyConfiguration(new AuthenticationAttemptConfiguration());
    }
}
