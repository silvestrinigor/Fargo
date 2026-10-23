using Fargo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

/// <summary>
/// Configures the EF Core mapping for
/// <see cref="ItemParentContainerHistoryPostgresTemporal"/>, the temporal history
/// of item containment.
/// </summary>
/// <remarks>
/// This configuration maps only what EF needs to materialize and relate the
/// entity: the surrogate key, the <c>tstzrange</c> validity column with its
/// default, and the foreign keys to the audited item and the recorded container.
/// The two behaviours that make the table temporal — rejecting overlapping
/// periods and opening/closing periods as an item moves — live in the database
/// (a GiST exclusion constraint and a trigger) and are created by raw SQL in the
/// corresponding migration, as explained in <see cref="Configure"/>.
/// </remarks>
public sealed class ItemParentContainerHistoryPostgresTemporalConfiguration : IEntityTypeConfiguration<ItemParentContainerHistoryPostgresTemporal>
{
    /// <summary>
    /// Applies the schema mapping for the item containment history table.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity type.</param>
    /// <remarks>
    /// <para>
    /// The validity column is given the default <c>tstzrange(now(), 'infinity',
    /// '[)')</c> so that a row created directly starts "now" and stays open until
    /// closed. The trigger that maintains history sets the period explicitly, so
    /// in practice the default only applies to the trigger's own inserts as a
    /// fallback.
    /// </para>
    /// <para>
    /// The item foreign key cascades on delete, matching the existing item history
    /// convention (<c>ItemMoviment</c>): a deleted item takes its history with it.
    /// The container foreign key is set to <see langword="null"/> on delete so that
    /// removing a former container does not erase the fact that the item once lived
    /// there — the period is preserved with a null container reference.
    /// </para>
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
