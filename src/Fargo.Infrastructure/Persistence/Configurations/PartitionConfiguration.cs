using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    internal class PartitionConfiguration : IEntityTypeConfiguration<Partition>
    {
        public void Configure(EntityTypeBuilder<Partition> builder)
        {
            builder
                .ToTable(t => t.IsTemporal());

            builder.HasKey(x => x.Guid);

            builder.HasOne(x => x.UpdatedBy).WithMany().HasForeignKey(x => x.UpdatedByUserGuid);
        }
    }
}