using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ItemEventLifecyclePayload : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_item_events_deletes_item_event_guid",
            table: "item_events");

        migrationBuilder.DropColumn(
            name: "deletes_item_event_guid",
            table: "item_events");

        migrationBuilder.DropColumn(
            name: "item_container_guid",
            table: "item_events");

        migrationBuilder.CreateTable(
            name: "item_lifecycle_events",
            columns: table => new
            {
                item_event_guid = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<double>(type: "double precision", nullable: false),
                item_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                no_container = table.Column<bool>(type: "boolean", nullable: false)
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

        migrationBuilder.CreateIndex(
            name: "ix_item_lifecycle_events_item_container_guid",
            table: "item_lifecycle_events",
            column: "item_container_guid");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "item_lifecycle_events");

        migrationBuilder.AddColumn<Guid>(
            name: "deletes_item_event_guid",
            table: "item_events",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "item_container_guid",
            table: "item_events",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_item_events_deletes_item_event_guid",
            table: "item_events",
            column: "deletes_item_event_guid");
    }
}
