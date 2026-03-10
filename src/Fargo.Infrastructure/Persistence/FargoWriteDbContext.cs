using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Converters;
using Fargo.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence
{
    public class FargoWriteDbContext(DbContextOptions<FargoWriteDbContext> options) : DbContext(options)
    {
        public DbSet<Article> Articles { get; set; }

        public DbSet<Item> Items { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<UserPermission> UserPermission { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

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
            modelBuilder.ApplyConfiguration(new ArticleConfiguration());

            modelBuilder.ApplyConfiguration(new ItemConfiguration());

            modelBuilder.ApplyConfiguration(new UserConfiguration());

            modelBuilder.ApplyConfiguration(new UserPermissionConfiguration());

            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        }
    }
}