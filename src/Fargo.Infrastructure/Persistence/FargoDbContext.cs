using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Identity;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Fargo.Infrastructure.Configurations;
using Fargo.Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence;

public class FargoDbContext(DbContextOptions<FargoDbContext> options) : DbContext(options)
{
    public DbSet<EntityEvent> EntityEvents { get; set; }

    public DbSet<EntityPartitionEvent> EntityPartitionEvents { get; set; }

    public DbSet<Article> Articles { get; set; }

    public DbSet<Item> Items { get; set; }

    public DbSet<ItemMovement> ItemMovements { get; set; }

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
        modelBuilder.ApplyConfiguration(new EntityEventConfiguration());

        modelBuilder.ApplyConfiguration(new EntityPartitionEventConfiguration());

        modelBuilder.ApplyConfiguration(new ArticleConfiguration());

        modelBuilder.ApplyConfiguration(new ItemConfiguration());

        modelBuilder.ApplyConfiguration(new ItemMovementConfiguration());

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
