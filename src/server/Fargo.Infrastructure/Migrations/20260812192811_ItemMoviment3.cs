using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ItemMoviment3 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_item_moviments_items_moved_to_container_guid",
            table: "item_moviments");

        migrationBuilder.AddColumn<bool>(
            name: "removed_from_containers",
            table: "item_moviments",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddForeignKey(
            name: "fk_item_moviments_items_moved_to_container_guid",
            table: "item_moviments",
            column: "moved_to_container_guid",
            principalTable: "items",
            principalColumn: "guid",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_item_moviments_items_moved_to_container_guid",
            table: "item_moviments");

        migrationBuilder.DropColumn(
            name: "removed_from_containers",
            table: "item_moviments");

        migrationBuilder.AddForeignKey(
            name: "fk_item_moviments_items_moved_to_container_guid",
            table: "item_moviments",
            column: "moved_to_container_guid",
            principalTable: "items",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);
    }
}
