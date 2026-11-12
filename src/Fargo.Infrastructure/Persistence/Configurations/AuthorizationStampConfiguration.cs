using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public sealed class AuthorizationStampConfiguration : IEntityTypeConfiguration<AuthorizationStamp>
{
    public void Configure(EntityTypeBuilder<AuthorizationStamp> builder)
    {
        builder.ToTable("authorization_stamps");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).ValueGeneratedNever();

        // The interceptor only ever updates this row, so it must exist from the first migration on.
        builder.HasData(new AuthorizationStamp(AuthorizationStamp.InitialValue));
    }
}
