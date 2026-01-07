using Fargo.Domain.Entities.Events.Abstracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder
                .HasKey(x => x.Guid);

            builder
                .HasIndex(x => new { x.ModelGuid, x.OccurredAt });

            builder
                .HasIndex(x => x.OccurredAt);

            builder
                .UseTptMappingStrategy();
        }
    }
}