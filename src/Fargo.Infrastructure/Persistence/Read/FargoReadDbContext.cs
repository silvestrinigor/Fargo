using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Models.UserModels;
using Fargo.Infrastructure.Persistence.Read.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Read
{
    public class FargoReadDbContext(DbContextOptions<FargoReadDbContext> options) : DbContext(options)
    {
        public DbSet<ArticleReadModel> Articles { get; set; }

        public DbSet<ItemReadModel> Items { get; set; }

        public DbSet<UserReadModel> Users { get; set; }

        public DbSet<PermissionReadModel> Permissions { get; set; }

        public DbSet<PartitionReadModel> Partitions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ArticleReadModelConfiguration());

            modelBuilder.ApplyConfiguration(new ItemReadModelConfiguration());

            modelBuilder.ApplyConfiguration(new UserReadModelConfiguration());

            modelBuilder.ApplyConfiguration(new PermissionReadModelConfiguration());

            modelBuilder.ApplyConfiguration(new PartitionReadModelConfiguration());
        }
    }
}
