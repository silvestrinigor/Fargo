using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ItemParentContainerHistory : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "last_parent_item_container_changed_at",
            table: "items",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "item_moviments",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                item_guid = table.Column<Guid>(type: "uuid", nullable: false),
                parent_item_contianer_guid = table.Column<Guid>(type: "uuid", nullable: true),
                valid_at = table.Column<NpgsqlRange<DateTimeOffset>>(type: "tstzrange", nullable: false),
                item_guid1 = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_item_moviments", x => x.guid);
                table.UniqueConstraint("ak_item_moviments_item_guid_valid_at", x => new { x.item_guid, x.valid_at });
                table.ForeignKey(
                    name: "fk_item_moviments_items_item_guid",
                    column: x => x.item_guid,
                    principalTable: "items",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_item_moviments_items_item_guid1",
                    column: x => x.item_guid1,
                    principalTable: "items",
                    principalColumn: "guid");
                table.ForeignKey(
                    name: "fk_item_moviments_items_parent_item_contianer_guid",
                    column: x => x.parent_item_contianer_guid,
                    principalTable: "items",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_item_moviments_item_guid1",
            table: "item_moviments",
            column: "item_guid1");

        migrationBuilder.CreateIndex(
            name: "ix_item_moviments_parent_item_contianer_guid",
            table: "item_moviments",
            column: "parent_item_contianer_guid");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "item_moviments");

        migrationBuilder.DropColumn(
            name: "last_parent_item_container_changed_at",
            table: "items");
    }
}
