using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ItemParentContainerHistoryRelation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_item_parent_containers_history_items_item_guid1",
            table: "item_parent_containers_history");

        migrationBuilder.DropIndex(
            name: "ix_item_parent_containers_history_item_guid1",
            table: "item_parent_containers_history");

        migrationBuilder.DropColumn(
            name: "item_guid1",
            table: "item_parent_containers_history");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "item_guid1",
            table: "item_parent_containers_history",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_item_parent_containers_history_item_guid1",
            table: "item_parent_containers_history",
            column: "item_guid1");

        migrationBuilder.AddForeignKey(
            name: "fk_item_parent_containers_history_items_item_guid1",
            table: "item_parent_containers_history",
            column: "item_guid1",
            principalTable: "items",
            principalColumn: "guid");
    }
}
