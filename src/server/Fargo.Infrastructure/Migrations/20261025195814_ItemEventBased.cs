using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ItemEventBased : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<byte>(
            name: "tracking_mode",
            table: "articles",
            type: "smallint",
            nullable: false,
            defaultValue: (byte)0);

        migrationBuilder.CreateTable(
            name: "item_events",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                event_type = table.Column<short>(type: "smallint", nullable: false),
                occured_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_item_events", x => x.guid);
                table.ForeignKey(
                    name: "fk_item_events_articles_article_guid",
                    column: x => x.article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "item_moviment_events",
            columns: table => new
            {
                item_event_guid = table.Column<Guid>(type: "uuid", nullable: false),
                from_item_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                from_no_container = table.Column<bool>(type: "boolean", nullable: false),
                to_item_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
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

        migrationBuilder.CreateIndex(
            name: "ix_item_events_article_guid",
            table: "item_events",
            column: "article_guid");

        migrationBuilder.CreateIndex(
            name: "ix_item_events_article_guid_occured_at",
            table: "item_events",
            columns: new[] { "article_guid", "occured_at" });

        migrationBuilder.CreateIndex(
            name: "ix_item_events_occured_at",
            table: "item_events",
            column: "occured_at");

        migrationBuilder.CreateIndex(
            name: "ix_item_moviment_events_from_item_container_guid",
            table: "item_moviment_events",
            column: "from_item_container_guid");

        migrationBuilder.CreateIndex(
            name: "ix_item_moviment_events_to_item_container_guid",
            table: "item_moviment_events",
            column: "to_item_container_guid");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "item_moviment_events");

        migrationBuilder.DropTable(
            name: "item_events");

        migrationBuilder.DropColumn(
            name: "tracking_mode",
            table: "articles");
    }
}
