using Fargo.Domain.Abstracts.Entities;
using Fargo.Domain.Entities;
using Fargo.Infrastructure.Persistence.Configurations;
using Fargo.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence
{
    public class FargoContext(DbContextOptions<FargoContext> options) : DbContext(options)
    {
        public DbSet<NamedEntity> Entities { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<AreaClosure> AreaClosure { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Item> ArticleItems { get; set; }
        public DbSet<Container> Containers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase("Fargo");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FargoContext).Assembly);
            modelBuilder.ApplyConfiguration(new EntityConfiguration());
            modelBuilder.ApplyConfiguration(new AreaConfiguration());
            modelBuilder.ApplyConfiguration(new AreaClosureConfiguration());
            modelBuilder.ApplyConfiguration(new ArticleConfiguration());
            modelBuilder.ApplyConfiguration(new ArticleItemConfiguration());
            modelBuilder.ApplyConfiguration(new ContainerConfiguration());
            modelBuilder.ApplyConfiguration(new PartitionConfiguration());
        }
    }
}