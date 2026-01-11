using Fargo.Application.Models.PartitionModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Read.Configurations
{
    public class PartitionReadModelConfiguration : IEntityTypeConfiguration<PartitionReadModel>
    {
        public void Configure(EntityTypeBuilder<PartitionReadModel> builder)
        {
            builder.ToTable(x => x.IsTemporal());

            builder.HasKey(x => x.Guid);
        }
    }
}
