using Fargo.Application.Models.UserGroupModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    public class UserGroupReadModelConfiguration : IEntityTypeConfiguration<UserGroupReadModel>
    {
        public void Configure(EntityTypeBuilder<UserGroupReadModel> builder)
        {
            builder.ToTable(t => t.IsTemporal());

            builder.HasKey(x => x.Guid);

            builder.HasAlternateKey(x => x.Nameid);

            builder.HasMany(x => x.UserGroupPermissions)
                .WithOne().HasForeignKey(x => x.UserGroupGuid);
        }
    }
}