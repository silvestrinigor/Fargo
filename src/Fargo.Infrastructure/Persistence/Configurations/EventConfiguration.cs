using Fargo.Domain.Entities.Events;
using Fargo.Domain.Entities.Events.Abstracts;
using Fargo.Domain.Enums;
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
                .UseTptMappingStrategy();
        }
    }
}