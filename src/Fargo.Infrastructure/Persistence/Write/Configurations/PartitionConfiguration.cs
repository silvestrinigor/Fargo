using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Write.Configurations
{
    internal class PartitionConfiguration : IEntityTypeConfiguration<Partition>
    {
        public void Configure(EntityTypeBuilder<Partition> builder)
        {
            builder
                .ToTable(t => t.IsTemporal());

            builder.HasKey(x => x.Guid);

            builder
                .HasMany(x => x.Articles)
                .WithMany();
        }
    }
}
