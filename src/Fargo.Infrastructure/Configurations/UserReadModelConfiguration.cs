using Fargo.Application.Models.UserModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    public class UserReadModelConfiguration : IEntityTypeConfiguration<UserReadModel>
    {
        public void Configure(EntityTypeBuilder<UserReadModel> builder)
        {
            builder.ToTable(t => t.IsTemporal());

            builder.HasKey(x => x.Guid);

            builder.HasAlternateKey(x => x.Nameid);

            builder.HasMany(x => x.UserPermissions)
                .WithOne().HasForeignKey(x => x.UserGuid);
        }
    }
}