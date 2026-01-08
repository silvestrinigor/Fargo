using Fargo.Domain.Entities.Events;
using Fargo.Domain.Entities.Events.Abstracts;
using Fargo.Domain.Entities.Models;
using Fargo.Domain.Entities.Models.Abstracts;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Converters;
using Fargo.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence
{
    public class FargoContext(DbContextOptions<FargoContext> options) : DbContext(options)
    {
        public DbSet<Model> Models { get; set; }

        public DbSet<Article> Articles { get; set; }

        public DbSet<Item> Items { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<ArticleCreatedEvent> ArticleCreatedEvents { get; set; }

        public DbSet<ArticleDeletedEvent> ArticleDeletedEvents { get; set; }

        public DbSet<ItemCreatedEvent> ItemCreatedEvents { get; set; }

        public DbSet<ItemDeletedEvent> ItemDeletedEvents { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<Name>()
                .HaveConversion<NameStringConverter>();

            configurationBuilder
                .Properties<Description>()
                .HaveConversion<DescriptionStringConverter>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ModelConfiguration());

            modelBuilder.ApplyConfiguration(new ArticleConfiguration());

            modelBuilder.ApplyConfiguration(new ItemConfiguration());

            modelBuilder.ApplyConfiguration(new EventConfiguration());
        }
    }
}