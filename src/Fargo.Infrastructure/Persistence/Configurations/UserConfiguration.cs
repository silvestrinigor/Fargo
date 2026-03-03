using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable(t => t.IsTemporal());

            builder.HasKey(x => x.Guid);

            builder.HasOne(x => x.UpdatedBy).WithMany().HasForeignKey(x => x.UpdatedByUserGuid);

            builder.HasAlternateKey(x => x.Nameid);

            builder
                .Property(x => x.Nameid)
                .IsRequired();

            builder
                .Property(x => x.Description)
                .IsRequired();

            builder.HasMany(x => x.Partitions).WithMany();

            builder.HasMany(x => x.PartitionsAccesses).WithMany();
        }
    }
}