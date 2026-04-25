using Fargo.Domain;
using Fargo.Domain.ApiClients;
using Fargo.Domain.Articles;
using Fargo.Domain.Barcodes;
using Fargo.Domain.Events;
using Fargo.Domain.Items;
using Fargo.Domain.Partitions;
using Fargo.Domain.Tokens;
using Fargo.Domain.Users;
using Fargo.Infrastructure.Configurations;
using Fargo.Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence;

public class FargoDbContext(DbContextOptions<FargoDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events { get; set; }

    public DbSet<ApiClient> ApiClients { get; set; }

    public DbSet<Article> Articles { get; set; }

    public DbSet<Barcode> Barcodes { get; set; }

    public DbSet<Item> Items { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<UserPermission> UserPermission { get; set; }

    public DbSet<UserGroup> UserGroups { get; set; }

    public DbSet<UserGroupPermission> UserGroupPermissions { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Partition> Partitions { get; set; }

    public DbSet<UserPartitionAccess> UserPartitionAccesses { get; set; }

    public DbSet<UserGroupPartitionAccess> UserGroupPartitionAccesses { get; set; }

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventConfiguration());

        modelBuilder.ApplyConfiguration(new ApiClientConfiguration());

        modelBuilder.ApplyConfiguration(new ArticleConfiguration());

        modelBuilder.ApplyConfiguration(new BarcodeConfiguration());

        modelBuilder.ApplyConfiguration(new ItemConfiguration());

        modelBuilder.ApplyConfiguration(new UserConfiguration());

        modelBuilder.ApplyConfiguration(new UserPermissionConfiguration());

        modelBuilder.ApplyConfiguration(new UserGroupConfiguration());

        modelBuilder.ApplyConfiguration(new UserGroupPermissionConfiguration());

        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

        modelBuilder.ApplyConfiguration(new PartitionConfiguration());

        modelBuilder.ApplyConfiguration(new UserPartitionAccessConfiguration());

        modelBuilder.ApplyConfiguration(new UserGroupPartitionAccessConfiguration());
    }
}
