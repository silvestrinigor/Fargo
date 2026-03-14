using Fargo.Application.Models.PartitionModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    public class PartitionReadModelConfiguration : IEntityTypeConfiguration<PartitionReadModel>
    {
        public void Configure(EntityTypeBuilder<PartitionReadModel> builder)
        {
            // Configure the table as a temporal table to enable historical tracking
            builder.ToTable(t => t.IsTemporal());

            // Configure the primary key
            builder.HasKey(x => x.Guid);

            // Configure required properties
            builder
                .Property(x => x.Name)
                .IsRequired();

            builder
                .Property(x => x.Description)
                .IsRequired();

            builder
                .Property(x => x.IsActive)
                .IsRequired();

            // Configure optional parent partition reference
            builder
                .Property(x => x.ParentPartitionGuid);

            builder
                .HasOne<PartitionReadModel>()
                .WithMany()
                .HasForeignKey(x => x.ParentPartitionGuid);
        }
    }
}