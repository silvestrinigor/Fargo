using Microsoft.EntityFrameworkCore.Migrations;

namespace Fargo.Infrastructure.Migrations;

/// <summary>
/// Migration helpers that encapsulate the database-side objects which turn
/// <c>item_parent_container_history</c> into an application-time temporal table.
/// </summary>
/// <remarks>
/// <para>
/// EF Core has no model-level API that <i>generates</i> a trigger (its
/// <c>HasTrigger</c> only informs EF that a trigger exists so it can adjust
/// <c>SaveChanges</c>; it never emits <c>CREATE TRIGGER</c>). The officially
/// supported ways to ship a trigger through migrations are a raw
/// <see cref="MigrationBuilder.Sql(string, bool)"/> call or a custom migration
/// operation.
/// </para>
/// <para>
/// This class takes the first approach but keeps the SQL out of the migration
/// body: the definition lives here, in one typed, reusable place, so a migration
/// reads declaratively as
/// <c>migrationBuilder.CreateItemParentContainerHistoryTrigger()</c> /
/// <c>migrationBuilder.DropItemParentContainerHistoryTrigger()</c>. This is the
/// closest equivalent to configuring the behaviour "in C#" that the current
/// EF/Npgsql stack allows.
/// </para>
/// </remarks>
internal static class ItemParentContainerHistoryTriggerMigrationExtensions
{
    private const string TriggerName = "tr_item_parent_container_history";
    private const string FunctionName = "fn_item_parent_container_history";
    private const string ExclusionConstraintName = "ex_item_parent_container_history_no_overlap";
    private const string HistoryTable = "item_parent_container_history";
    private const string ItemsTable = "items";

    /// <summary>
    /// Creates the <c>btree_gist</c> extension, the non-overlap exclusion
    /// constraint, and the history-maintenance trigger (and its function).
    /// </summary>
    /// <param name="migrationBuilder">The builder used to emit the SQL.</param>
    public static void CreateItemParentContainerHistoryTrigger(this MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        // btree_gist lets a GiST index combine an equality operator (=) on a
        // scalar column with the overlap operator (&&) on a range column, which
        // is what the exclusion constraint needs.
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

        // Reject any two history rows for the same item whose validity periods
        // overlap. Equivalent to PostgreSQL 18's
        // PRIMARY KEY (item_guid, valid_period WITHOUT OVERLAPS).
        migrationBuilder.Sql(
            $"""
            ALTER TABLE {HistoryTable}
                ADD CONSTRAINT {ExclusionConstraintName}
                EXCLUDE USING gist (item_guid WITH =, valid_period WITH &&);
            """);

        // Trigger function: maintains the containment history of an item.
        //
        // PostgreSQL has no core SYSTEM VERSIONING, so the "close the current
        // period, open a new one" behaviour is implemented here explicitly.
        //
        //   * On INSERT of an item, open the first period reflecting the item's
        //     initial parent container (if any).
        //   * On UPDATE where parent_item_container_guid actually changed, close
        //     the currently open period (upper bound = infinity) at the current
        //     instant and open a new period from that instant.
        //
        // clock_timestamp() (wall-clock, advances within a transaction) is used
        // instead of now()/transaction_timestamp() (fixed for the whole
        // transaction) so that an insert followed by an update of the same item
        // in a single transaction produces strictly increasing, non-empty,
        // non-overlapping periods.
        migrationBuilder.Sql(
            $"""
            CREATE OR REPLACE FUNCTION {FunctionName}()
            RETURNS TRIGGER
            LANGUAGE plpgsql
            AS $$
            DECLARE
                change_instant timestamptz := clock_timestamp();
            BEGIN
                IF (TG_OP = 'INSERT') THEN
                    INSERT INTO {HistoryTable}
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
                        UPDATE {HistoryTable}
                        SET valid_period = tstzrange(lower(valid_period), change_instant, '[)')
                        WHERE item_guid = NEW.guid
                          AND upper(valid_period) = 'infinity';

                        -- Open the new period reflecting the new parent.
                        INSERT INTO {HistoryTable}
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
            $"""
            CREATE TRIGGER {TriggerName}
            AFTER INSERT OR UPDATE OF parent_item_container_guid ON {ItemsTable}
            FOR EACH ROW
            EXECUTE FUNCTION {FunctionName}();
            """);
    }

    /// <summary>
    /// Drops the trigger, its function, and the exclusion constraint. The
    /// <c>btree_gist</c> extension is intentionally left installed, since
    /// dropping an extension other objects may depend on is rarely desirable in
    /// a rollback.
    /// </summary>
    /// <param name="migrationBuilder">The builder used to emit the SQL.</param>
    public static void DropItemParentContainerHistoryTrigger(this MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.Sql($"DROP TRIGGER IF EXISTS {TriggerName} ON {ItemsTable};");

        migrationBuilder.Sql($"DROP FUNCTION IF EXISTS {FunctionName}();");

        migrationBuilder.Sql(
            $"""
            ALTER TABLE {HistoryTable}
                DROP CONSTRAINT IF EXISTS {ExclusionConstraintName};
            """);
    }
}
