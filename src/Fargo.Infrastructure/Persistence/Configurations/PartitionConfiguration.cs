using Fargo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public class PartitionConfiguration : IEntityTypeConfiguration<Partition>
{
    public void Configure(EntityTypeBuilder<Partition> builder)
    {
        builder.Navigation(x => x.Entities)
               .UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}
