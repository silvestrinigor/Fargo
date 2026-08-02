using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ItemParentContainerTableName : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_items_items_parent_container_guid",
            table: "items");

        migrationBuilder.RenameColumn(
            name: "parent_container_guid",
            table: "items",
            newName: "parent_item_container_guid");

        migrationBuilder.RenameIndex(
            name: "ix_items_parent_container_guid",
            table: "items",
            newName: "ix_items_parent_item_container_guid");

        migrationBuilder.AddForeignKey(
            name: "fk_items_items_parent_item_container_guid",
            table: "items",
            column: "parent_item_container_guid",
            principalTable: "items",
            principalColumn: "guid",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_items_items_parent_item_container_guid",
            table: "items");

        migrationBuilder.RenameColumn(
            name: "parent_item_container_guid",
            table: "items",
            newName: "parent_container_guid");

        migrationBuilder.RenameIndex(
            name: "ix_items_parent_item_container_guid",
            table: "items",
            newName: "ix_items_parent_container_guid");

        migrationBuilder.AddForeignKey(
            name: "fk_items_items_parent_container_guid",
            table: "items",
            column: "parent_container_guid",
            principalTable: "items",
            principalColumn: "guid",
            onDelete: ReferentialAction.SetNull);
    }
}
