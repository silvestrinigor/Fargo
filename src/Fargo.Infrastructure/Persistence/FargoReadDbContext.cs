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
    public class FargoReadDbContext(DbContextOptions<FargoReadDbContext> options) : DbContext(options)
    {
        public DbSet<ArticleReadModel> Articles { get; set; }

        public DbSet<ItemReadModel> Items { get; set; }

        public DbSet<UserReadModel> Users { get; set; }

        public DbSet<UserPermissionReadModel> UserPermission { get; set; }

        public DbSet<UserGroupReadModel> UserGroups { get; set; }

        public DbSet<UserGroupPermissionReadModel> UserGroupPermissions { get; set; }

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