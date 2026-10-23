using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <summary>
    /// Installs the database-side machinery that turns
    /// <c>item_parent_container_history</c> into an application-time temporal
    /// table: a GiST exclusion constraint that forbids overlapping periods for the
    /// same item, and a trigger on the <c>items</c> table that opens and closes
    /// those periods as an item's parent container changes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These objects are written in raw SQL because they have no equivalent in the
    /// EF Core model on the current stack. The typed temporal API
    /// (<c>WithoutOverlaps()</c> / <c>WithPeriod()</c>) requires
    /// Npgsql.EFCore.PostgreSQL v11 and PostgreSQL 18; the GiST exclusion
    /// constraint approach used here achieves the same non-overlap guarantee on
    /// earlier versions, and PostgreSQL's lack of core system versioning means the
    /// open/close of periods must be performed explicitly by the trigger.
    /// </para>
    /// <para>
    /// The trigger is the single writer of history rows: the application never
    /// inserts into <c>item_parent_container_history</c> directly.
    /// </para>
    /// </remarks>
    public partial class ItemParentContainerHistoryTrigger : Migration
    {
        /// <summary>
        /// Creates the <c>btree_gist</c> extension, the non-overlap exclusion
        /// constraint, and the history-maintenance trigger (and its function).
        /// </summary>
        /// <param name="migrationBuilder">The builder used to emit the SQL.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateItemParentContainerHistoryTrigger();
        }

        /// <summary>
        /// Drops the trigger, its function, and the exclusion constraint. The
        /// <c>btree_gist</c> extension is intentionally left installed.
        /// </summary>
        /// <param name="migrationBuilder">The builder used to emit the SQL.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropItemParentContainerHistoryTrigger();
        }
    }
}
