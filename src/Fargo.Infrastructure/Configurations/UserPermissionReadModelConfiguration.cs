using Fargo.Application.Models.UserModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    public class UserPermissionReadModelConfiguration : IEntityTypeConfiguration<UserPermissionReadModel>
    {
        public void Configure(EntityTypeBuilder<UserPermissionReadModel> builder)
        {
            builder.ToTable(x => x.IsTemporal());

            builder.HasKey(x => new { x.UserGuid, x.Action });
        }
    }
}