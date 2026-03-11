using Fargo.Application.Models.UserGroupModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    public class UserGroupPermissionReadModelConfiguration
        : IEntityTypeConfiguration<UserGroupPermissionReadModel>
    {
        public void Configure(EntityTypeBuilder<UserGroupPermissionReadModel> builder)
        {
            builder.ToTable(t => t.IsTemporal());

            builder.HasKey(x => x.Guid);

            builder.HasAlternateKey(x => new { x.UserGroupGuid, x.Action });
        }
    }
}