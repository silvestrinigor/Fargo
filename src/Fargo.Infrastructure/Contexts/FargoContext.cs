using Microsoft.EntityFrameworkCore;
using Fargo.Core.Entities;
using Fargo.Infrastructure.Configurations;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Infrastructure.Contexts;

public class FargoContext(DbContextOptions<FargoContext> options) : DbContext(options)
{
    public DbSet<Article> Articles { get; set; }
    public DbSet<Container> Containers { get; set; }
    public DbSet<DetailedEntity> DetailedEntities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("Fargo");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FargoContext).Assembly);
        modelBuilder.ApplyConfiguration(new ArticleConfiguration());
        modelBuilder.ApplyConfiguration(new ContainerConfiguration());
        modelBuilder.ApplyConfiguration(new DetailedEntityConfiguration());
    }
}
