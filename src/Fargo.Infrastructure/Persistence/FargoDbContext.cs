using Fargo.Core.Articles;
using Fargo.Core.Identity;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.Security;
using Fargo.Core.Shared.Barcodes;
using Fargo.Core.Shared.Informations;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Fargo.Infrastructure.Configurations;
using Fargo.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using UnitsNet;

namespace Fargo.Infrastructure.Persistence;

public class FargoDbContext(DbContextOptions<FargoDbContext> options) : DbContext(options)
{
    public DbSet<Article> Articles { get; set; }

    public DbSet<Item> Items { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<UserGroup> UserGroups { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Partition> Partitions { get; set; }

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
        modelBuilder.ApplyConfiguration(new ArticleConfiguration());

        modelBuilder.ApplyConfiguration(new ArticleBarcodeConfiguration());

        modelBuilder.ApplyConfiguration(new ArticleDimensionConfiguration());

        modelBuilder.ApplyConfiguration(new ArticleVariationConfiguration());

        modelBuilder.ApplyConfiguration(new ArticlePackConfiguration());

        modelBuilder.ApplyConfiguration(new ArticleKitComponentConfiguration());

        modelBuilder.ApplyConfiguration(new ArticleContainerConfiguration());

        modelBuilder.ApplyConfiguration(new ItemConfiguration());

        modelBuilder.ApplyConfiguration(new UserConfiguration());

        modelBuilder.ApplyConfiguration(new UserAuthenticationConfiguration());

        modelBuilder.ApplyConfiguration(new UserUserGroupConfiguration());

        modelBuilder.ApplyConfiguration(new UserPartitionConfiguration());

        modelBuilder.ApplyConfiguration(new UserPartitionAccessConfiguration());

        modelBuilder.ApplyConfiguration(new UserGroupConfiguration());

        modelBuilder.ApplyConfiguration(new UserGroupPartitionConfiguration());

        modelBuilder.ApplyConfiguration(new UserGroupPartitionAccessConfiguration());

        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

        modelBuilder.ApplyConfiguration(new PartitionConfiguration());
    }
}
