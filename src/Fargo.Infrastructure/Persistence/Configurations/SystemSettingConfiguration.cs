using Fargo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
    {
        public void Configure(EntityTypeBuilder<SystemSetting> builder)
        {
            builder.HasIndex(x => x.Name)
                .IsUnique();

            builder.Property(x => x.ValueType)
                .IsRequired();

            builder.Property(x => x.BoolValue);

            builder.Property(x => x.StringValue);

            builder.Property(x => x.GuidValue);
        }
    }
}
