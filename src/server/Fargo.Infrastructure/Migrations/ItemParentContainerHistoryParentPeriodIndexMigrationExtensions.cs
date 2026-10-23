using Microsoft.EntityFrameworkCore.Migrations;

namespace Fargo.Infrastructure.Migrations;

/// <summary>
/// Migration helper for the GiST index that accelerates the temporal
/// container-walk over <c>item_parent_container_history</c>.
/// </summary>
/// <remarks>
/// <para>
/// The point-in-time inventory and location queries recurse over the history
/// table joining on the parent side with a range containment filter:
/// <c>child.parent_item_container_guid = parent.guid AND child.valid_period @> at</c>.
/// A plain B-tree on <c>parent_item_container_guid</c> can satisfy the equality
/// but must then filter the range; a GiST index over
/// <c>(parent_item_container_guid, valid_period)</c> (via the <c>btree_gist</c>
/// extension) turns each recursion step into a single index range probe.
/// </para>
/// <para>
/// This mirrors the exclusion-constraint index on <c>(item_guid, valid_period)</c>
/// — which already serves per-item temporal lookups — but keyed on the parent
/// side the container-walk joins on. It is expressed in raw SQL because EF Core
/// cannot model a GiST index combining an equality and a range column.
/// </para>
/// </remarks>
internal static class ItemParentContainerHistoryParentPeriodIndexMigrationExtensions
{
    private const string IndexName = "ix_item_parent_container_history_parent_valid_period";
    private const string HistoryTable = "item_parent_container_history";

    /// <summary>
    /// Creates the GiST index on <c>(parent_item_container_guid, valid_period)</c>.
    /// Assumes the <c>btree_gist</c> extension is already installed (it is, by the
    /// exclusion-constraint migration).
    /// </summary>
    /// <param name="migrationBuilder">The builder used to emit the SQL.</param>
    public static void CreateItemParentContainerHistoryParentPeriodIndex(this MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        // btree_gist is required to combine the scalar equality column with the
        // range column in one GiST index. The exclusion-constraint migration
        // already installs it; this is defensive and idempotent.
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

        migrationBuilder.Sql(
            $"""
            CREATE INDEX {IndexName}
                ON {HistoryTable}
                USING gist (parent_item_container_guid, valid_period);
            """);
    }

    /// <summary>
    /// Drops the GiST index.
    /// </summary>
    /// <param name="migrationBuilder">The builder used to emit the SQL.</param>
    public static void DropItemParentContainerHistoryParentPeriodIndex(this MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.Sql($"DROP INDEX IF EXISTS {IndexName};");
    }
}
