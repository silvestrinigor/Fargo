using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnifyItemEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "from_item_container_guid",
                table: "item_events",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "from_no_container",
                table: "item_events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "item_guid",
                table: "item_events",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "quantity",
                table: "item_events",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "to_item_container_guid",
                table: "item_events",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "to_no_container",
                table: "item_events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_item_events_from_item_container_guid",
                table: "item_events",
                column: "from_item_container_guid");

            migrationBuilder.CreateIndex(
                name: "ix_item_events_item_guid",
                table: "item_events",
                column: "item_guid");

            migrationBuilder.CreateIndex(
                name: "ix_item_events_to_item_container_guid",
                table: "item_events",
                column: "to_item_container_guid");

            migrationBuilder.AddForeignKey(
                name: "fk_item_events_items_from_item_container_guid",
                table: "item_events",
                column: "from_item_container_guid",
                principalTable: "items",
                principalColumn: "guid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_item_events_items_to_item_container_guid",
                table: "item_events",
                column: "to_item_container_guid",
                principalTable: "items",
                principalColumn: "guid",
                onDelete: ReferentialAction.SetNull);

            // Hand-written: copies the payload tables into item_events before they are dropped.
            migrationBuilder.Sql(
                """
                UPDATE item_events AS e
                SET quantity = m.quantity,
                    from_item_container_guid = m.from_item_container_guid,
                    from_no_container = m.from_no_container,
                    to_item_container_guid = m.to_item_container_guid,
                    to_no_container = m.to_no_container
                FROM item_moviment_events AS m
                WHERE m.item_event_guid = e.guid;
                """);

            // A creation has no origin and a deletion has no destination.
            migrationBuilder.Sql(
                """
                UPDATE item_events AS e
                SET quantity = l.quantity,
                    from_item_container_guid = CASE WHEN e.item_event_type = 3 THEN l.item_container_guid END,
                    from_no_container = CASE WHEN e.item_event_type = 3 THEN l.no_container ELSE true END,
                    to_item_container_guid = CASE WHEN e.item_event_type = 2 THEN l.item_container_guid END,
                    to_no_container = CASE WHEN e.item_event_type = 2 THEN l.no_container ELSE true END
                FROM item_lifecycle_events AS l
                WHERE l.item_event_guid = e.guid;
                """);

            // The origin of each old item movement is the destination of the previous one.
            migrationBuilder.Sql(
                """
                WITH moved AS (
                    SELECT
                        gen_random_uuid() AS guid,
                        i.article_guid,
                        mv.item_guid,
                        mv.occurred_at,
                        mv.moved_to_container_guid,
                        mv.removed_from_containers,
                        LAG(mv.moved_to_container_guid) OVER w AS previous_container_guid,
                        COALESCE(LAG(mv.removed_from_containers) OVER w, true) AS previous_removed
                    FROM item_moviments AS mv
                    INNER JOIN items AS i ON i.guid = mv.item_guid
                    WINDOW w AS (PARTITION BY mv.item_guid ORDER BY mv.occurred_at)
                ),
                new_events AS (
                    INSERT INTO events (guid, event_type, occured_at)
                    SELECT guid, 0, occurred_at FROM moved
                    RETURNING guid
                )
                INSERT INTO item_events
                    (guid, article_guid, item_event_type, item_guid, quantity,
                     from_item_container_guid, from_no_container,
                     to_item_container_guid, to_no_container)
                SELECT
                    guid, article_guid, 1, item_guid, 1,
                    previous_container_guid, previous_removed,
                    moved_to_container_guid, removed_from_containers
                FROM moved;
                """);

            migrationBuilder.DropTable(
                name: "item_lifecycle_events");

            migrationBuilder.DropTable(
                name: "item_moviment_events");

            migrationBuilder.DropTable(
                name: "item_moviments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "item_lifecycle_events",
                columns: table => new
                {
                    item_event_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    item_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    no_container = table.Column<bool>(type: "boolean", nullable: false),
                    quantity = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_lifecycle_events", x => x.item_event_guid);
                    table.ForeignKey(
                        name: "fk_item_lifecycle_events_item_events_item_event_guid",
                        column: x => x.item_event_guid,
                        principalTable: "item_events",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_item_lifecycle_events_items_item_container_guid",
                        column: x => x.item_container_guid,
                        principalTable: "items",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "item_moviment_events",
                columns: table => new
                {
                    item_event_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    from_item_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    to_item_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    from_no_container = table.Column<bool>(type: "boolean", nullable: false),
                    quantity = table.Column<double>(type: "double precision", nullable: false),
                    to_no_container = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_moviment_events", x => x.item_event_guid);
                    table.ForeignKey(
                        name: "fk_item_moviment_events_item_events_item_event_guid",
                        column: x => x.item_event_guid,
                        principalTable: "item_events",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_item_moviment_events_items_from_item_container_guid",
                        column: x => x.from_item_container_guid,
                        principalTable: "items",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_item_moviment_events_items_to_item_container_guid",
                        column: x => x.to_item_container_guid,
                        principalTable: "items",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "item_moviments",
                columns: table => new
                {
                    item_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    moved_to_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    removed_from_containers = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_moviments", x => new { x.item_guid, x.occurred_at });
                    table.ForeignKey(
                        name: "fk_item_moviments_items_item_guid",
                        column: x => x.item_guid,
                        principalTable: "items",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_item_moviments_items_moved_to_container_guid",
                        column: x => x.moved_to_container_guid,
                        principalTable: "items",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_item_lifecycle_events_item_container_guid",
                table: "item_lifecycle_events",
                column: "item_container_guid");

            migrationBuilder.CreateIndex(
                name: "ix_item_moviment_events_from_item_container_guid",
                table: "item_moviment_events",
                column: "from_item_container_guid");

            migrationBuilder.CreateIndex(
                name: "ix_item_moviment_events_to_item_container_guid",
                table: "item_moviment_events",
                column: "to_item_container_guid");

            migrationBuilder.CreateIndex(
                name: "ix_item_moviments_moved_to_container_guid",
                table: "item_moviments",
                column: "moved_to_container_guid");

            // Hand-written: copies the events back into the payload tables.
            migrationBuilder.Sql(
                """
                INSERT INTO item_moviment_events
                    (item_event_guid, quantity, from_item_container_guid, from_no_container,
                     to_item_container_guid, to_no_container)
                SELECT guid, quantity, from_item_container_guid, from_no_container,
                       to_item_container_guid, to_no_container
                FROM item_events
                WHERE item_guid IS NULL AND item_event_type = 1;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO item_lifecycle_events
                    (item_event_guid, quantity, item_container_guid, no_container)
                SELECT guid, quantity,
                       CASE WHEN item_event_type = 2 THEN to_item_container_guid ELSE from_item_container_guid END,
                       CASE WHEN item_event_type = 2 THEN to_no_container ELSE from_no_container END
                FROM item_events
                WHERE item_guid IS NULL AND item_event_type IN (2, 3);
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO item_moviments (item_guid, occurred_at, moved_to_container_guid, removed_from_containers)
                SELECT e.item_guid, ev.occured_at, e.to_item_container_guid, e.to_no_container
                FROM item_events AS e
                INNER JOIN events AS ev ON ev.guid = e.guid
                INNER JOIN items AS i ON i.guid = e.item_guid
                WHERE e.item_event_type = 1 OR (e.item_event_type = 2 AND NOT e.to_no_container);
                """);

            // Events of items have no place in the old schema.
            migrationBuilder.Sql(
                """
                DELETE FROM events
                WHERE guid IN (SELECT guid FROM item_events WHERE item_guid IS NOT NULL);
                """);
            migrationBuilder.DropForeignKey(
                name: "fk_item_events_items_from_item_container_guid",
                table: "item_events");

            migrationBuilder.DropForeignKey(
                name: "fk_item_events_items_to_item_container_guid",
                table: "item_events");

            migrationBuilder.DropIndex(
                name: "ix_item_events_from_item_container_guid",
                table: "item_events");

            migrationBuilder.DropIndex(
                name: "ix_item_events_item_guid",
                table: "item_events");

            migrationBuilder.DropIndex(
                name: "ix_item_events_to_item_container_guid",
                table: "item_events");

            migrationBuilder.DropColumn(
                name: "from_item_container_guid",
                table: "item_events");

            migrationBuilder.DropColumn(
                name: "from_no_container",
                table: "item_events");

            migrationBuilder.DropColumn(
                name: "item_guid",
                table: "item_events");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "item_events");

            migrationBuilder.DropColumn(
                name: "to_item_container_guid",
                table: "item_events");

            migrationBuilder.DropColumn(
                name: "to_no_container",
                table: "item_events");
        }
    }
}
