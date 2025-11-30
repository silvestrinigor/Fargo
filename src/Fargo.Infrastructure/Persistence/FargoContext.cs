using Fargo.Domain.Entities;
using Fargo.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence
{
    public class FargoContext(DbContextOptions<FargoContext> options) : DbContext(options)
    {
        public DbSet<Article> Articles { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Container> Containers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase("Fargo");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FargoContext).Assembly);
            modelBuilder.ApplyConfiguration(new EntityConfiguration());
            modelBuilder.ApplyConfiguration(new AreaConfiguration());
            modelBuilder.ApplyConfiguration(new ArticleConfiguration());
            modelBuilder.ApplyConfiguration(new ItemConfiguration());
            modelBuilder.ApplyConfiguration(new ContainerConfiguration());
        }
    }
}