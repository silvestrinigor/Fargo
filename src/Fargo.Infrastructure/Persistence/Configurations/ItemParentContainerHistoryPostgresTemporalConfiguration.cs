using Fargo.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public sealed class ItemParentContainerHistoryPostgresTemporalConfiguration : IEntityTypeConfiguration<ItemParentContainerHistoryPostgresTemporal>
{
    /// <summary>
    /// The validity column defaults to <c>tstzrange(now(), 'infinity', '[)')</c>, so a row created directly
    /// starts now and stays open until closed. The history trigger sets the period explicitly, so the default is
    /// only a fallback.
    /// </summary>
    /// <remarks>
    /// The item foreign key cascades on delete, like <c>ItemMoviment</c>. The container foreign key is set to
    /// <see langword="null"/> on delete, so removing a former container keeps the fact that the item lived there.
    /// </remarks>
    public void Configure(EntityTypeBuilder<ItemParentContainerHistoryPostgresTemporal> builder)
    {
        builder.ToTable("item_parent_container_history");

        builder.HasKey(i => i.Id);

        // Non-overlap enforcement and history population are handled in the
        // database, not through the typed EF temporal API.
        //
        // The typed API (HasKey(...).WithoutOverlaps() + WithPeriod()) requires
        // Npgsql.EFCore.PostgreSQL v11 (currently preview, targets net11.0 + EF
        // Core 11) and PostgreSQL 18+. Until that stack is adopted, the same
        // guarantees are provided by:
        //   * a GiST exclusion constraint (item_guid WITH =, valid_period WITH &&)
        //     backed by the btree_gist extension, which rejects overlapping
        //     periods for the same item; and
        //   * a trigger on the "items" table that closes the current open period
        //     and opens a new one whenever an item's parent container changes.
        // Both are created in raw SQL by the migration, so they are intentionally
        // not expressed here.
        //
        // PostgreSQL has no core SYSTEM VERSIONING, so the close/open of periods
        // must be performed explicitly; here it is done by the database trigger.

        builder
        .Property(x => x.ValidPeriod)
        .HasDefaultValueSql("tstzrange(now(), 'infinity', '[)')");

        builder
        .HasOne(x => x.Item)
        .WithMany()
        .HasForeignKey(x => x.ItemGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(x => x.ParentItemContainer)
        .WithMany()
        .HasForeignKey(x => x.ParentItemContainerGuid)
        .OnDelete(DeleteBehavior.SetNull);
    }
}
