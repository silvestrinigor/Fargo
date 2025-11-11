using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;
using Fargo.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence;

public class FargoContext(DbContextOptions<FargoContext> options) : DbContext(options)
{
    public DbSet<Entity> Entities { get; set; }
    public DbSet<Area> Areas { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Container> Containers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseInMemoryDatabase("Fargo");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FargoContext).Assembly);
        modelBuilder.ApplyConfiguration(new EntityConfiguration());
        modelBuilder.ApplyConfiguration(new AreaConfiguration());
        modelBuilder.ApplyConfiguration(new ArticleConfiguration());
        modelBuilder.ApplyConfiguration(new ContainerConfiguration());
    }
}
