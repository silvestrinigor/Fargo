using Microsoft.EntityFrameworkCore;
using Fargo.Core.Entities;
using Fargo.Infrastructure.Configurations;

namespace Fargo.Infrastructure.Contexts;

public class FagoContext(DbContextOptions<FagoContext> options) : DbContext(options)
{
    public DbSet<Article> Articles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("Fargo");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FagoContext).Assembly);
        modelBuilder.ApplyConfiguration(new ArticleConfiguration());
    }
}
 