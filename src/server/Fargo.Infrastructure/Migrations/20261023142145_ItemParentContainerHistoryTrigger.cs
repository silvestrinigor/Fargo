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
            // btree_gist lets a GiST index combine an equality operator (=) on a
            // scalar column with the overlap operator (&&) on a range column,
            // which is what the exclusion constraint below needs.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            // Reject any two history rows for the same item whose validity
            // periods overlap. This is the equivalent of PostgreSQL 18's
            // PRIMARY KEY (item_guid, valid_period WITHOUT OVERLAPS).
            migrationBuilder.Sql(
                """
                ALTER TABLE item_parent_container_history
                    ADD CONSTRAINT ex_item_parent_container_history_no_overlap
                    EXCLUDE USING gist (item_guid WITH =, valid_period WITH &&);
                """);

            // Trigger function: maintains the containment history of an item.
            //
            // PostgreSQL has no core SYSTEM VERSIONING, so the "close the current
            // period, open a new one" behaviour is implemented here explicitly.
            //
            //   * On INSERT of an item, open the first period reflecting the
            //     item's initial parent container (if any).
            //   * On UPDATE where parent_item_container_guid actually changed,
            //     close the currently open period (upper bound = infinity) at the
            //     current instant and open a new period from that instant.
            //
            // clock_timestamp() (wall-clock, advances within a transaction) is
            // used instead of now()/transaction_timestamp() (fixed for the whole
            // transaction) so that an insert followed by an update of the same
            // item in a single transaction produces strictly increasing, non
            // empty, non-overlapping periods.
            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION fn_item_parent_container_history()
                RETURNS TRIGGER
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    change_instant timestamptz := clock_timestamp();
                BEGIN
                    IF (TG_OP = 'INSERT') THEN
                        INSERT INTO item_parent_container_history
                            (item_guid, parent_item_container_guid, removed_from_containers, valid_period)
                        VALUES
                            (NEW.guid,
                             NEW.parent_item_container_guid,
                             NEW.parent_item_container_guid IS NULL,
                             tstzrange(change_instant, 'infinity', '[)'));

                        RETURN NEW;
                    END IF;

                    IF (TG_OP = 'UPDATE') THEN
                        IF (NEW.parent_item_container_guid IS DISTINCT FROM OLD.parent_item_container_guid) THEN
                            -- Close the currently open period for this item.
                            UPDATE item_parent_container_history
                            SET valid_period = tstzrange(lower(valid_period), change_instant, '[)')
                            WHERE item_guid = NEW.guid
                              AND upper(valid_period) = 'infinity';

                            -- Open the new period reflecting the new parent.
                            INSERT INTO item_parent_container_history
                                (item_guid, parent_item_container_guid, removed_from_containers, valid_period)
                            VALUES
                                (NEW.guid,
                                 NEW.parent_item_container_guid,
                                 NEW.parent_item_container_guid IS NULL,
                                 tstzrange(change_instant, 'infinity', '[)'));
                        END IF;

                        RETURN NEW;
                    END IF;

                    RETURN NEW;
                END;
                $$;
                """);

            migrationBuilder.Sql(
                """
                CREATE TRIGGER tr_item_parent_container_history
                AFTER INSERT OR UPDATE OF parent_item_container_guid ON items
                FOR EACH ROW
                EXECUTE FUNCTION fn_item_parent_container_history();
                """);
        }

        /// <summary>
        /// Drops the trigger, its function, and the exclusion constraint. The
        /// <c>btree_gist</c> extension is intentionally left installed.
        /// </summary>
        /// <param name="migrationBuilder">The builder used to emit the SQL.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS tr_item_parent_container_history ON items;");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_item_parent_container_history();");

            migrationBuilder.Sql(
                """
                ALTER TABLE item_parent_container_history
                    DROP CONSTRAINT IF EXISTS ex_item_parent_container_history_no_overlap;
                """);

            // btree_gist is intentionally left installed: dropping an extension
            // other objects may depend on is risky and rarely desirable in a
            // rollback.
        }
    }
}
