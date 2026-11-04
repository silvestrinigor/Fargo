using Fargo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

/// <summary>
/// Maps the shared <c>entities</c> table that is the root of the Table-Per-Type
/// (TPT) hierarchy. Every concrete aggregate deriving from <see cref="Entity"/>
/// contributes exactly one row here, keyed by its <see cref="Entity.Guid"/> and
/// tagged with its <see cref="Entity.EntityType"/>.
/// </summary>
public sealed class EntityConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        // Root of the TPT hierarchy: the base lives in "entities" and each
        // derived aggregate is mapped to its own table whose primary key is also
        // a foreign key back to entities.guid.
        builder.UseTptMappingStrategy();

        builder.ToTable("entities");

        builder.HasKey(x => x.Guid);

        builder.Property(x => x.EntityType);

        builder.HasIndex(x => x.EntityType);
    }
}
