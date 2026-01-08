using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder
                .ToTable(t => t.IsTemporal());

            builder
                .HasKey(x => x.Guid);

            builder
                .Property(x => x.Name)
                .IsRequired();

            builder
                .HasMany(x => x.Permissions)
                .WithOne(x => x.User);

            builder
                .Property(x => x.Description)
                .IsRequired();
        }
    }
}
