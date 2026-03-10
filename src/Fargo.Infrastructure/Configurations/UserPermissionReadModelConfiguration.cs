using Fargo.Application.Models.UserModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    public class UserPermissionReadModelConfiguration : IEntityTypeConfiguration<UserPermissionReadModel>
    {
        public void Configure(EntityTypeBuilder<UserPermissionReadModel> builder)
        {
            builder.ToTable(t => t.IsTemporal());

            builder.HasKey(x => x.Guid);

            builder.HasAlternateKey(x => new { x.UserGuid, x.Action });
        }
    }
}