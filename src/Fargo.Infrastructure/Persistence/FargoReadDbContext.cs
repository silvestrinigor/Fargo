using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Models.UserModels;
using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Persistence.Configurations;
using Fargo.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence
{
    public class FargoReadDbContext(DbContextOptions<FargoReadDbContext> options) : DbContext(options)
    {
        public DbSet<ArticleReadModel> Articles { get; set; }

        public DbSet<ItemReadModel> Items { get; set; }

        public DbSet<UserReadModel> Users { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<ActionType>()
                .HaveConversion<string>();

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
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ArticleReadModelConfiguration());

            modelBuilder.ApplyConfiguration(new ItemReadModelConfiguration());

            modelBuilder.ApplyConfiguration(new UserReadModelConfiguration());
        }
    }
}