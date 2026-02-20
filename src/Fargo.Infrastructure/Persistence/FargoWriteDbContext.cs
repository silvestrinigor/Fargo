using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Persistence.Converters;
using Fargo.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence
{
    public class FargoWriteDbContext(DbContextOptions<FargoWriteDbContext> options) : DbContext(options)
    {
        public DbSet<Article> Articles { get; set; }

        public DbSet<Item> Items { get; set; }

        public DbSet<User> Users { get; set; }

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
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ArticleConfiguration());

            modelBuilder.ApplyConfiguration(new ItemConfiguration());

            modelBuilder.ApplyConfiguration(new UserConfiguration());

            modelBuilder.ApplyConfiguration(new PermissionConfiguration());

            modelBuilder.ApplyConfiguration(new PartitionConfiguration());
        }
    }
}