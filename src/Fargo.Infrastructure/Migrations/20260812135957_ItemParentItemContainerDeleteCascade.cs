using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemParentItemContainerDeleteCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_items_items_parent_item_container_guid",
                table: "items");

            migrationBuilder.AddForeignKey(
                name: "fk_items_items_parent_item_container_guid",
                table: "items",
                column: "parent_item_container_guid",
                principalTable: "items",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_items_items_parent_item_container_guid",
                table: "items");

            migrationBuilder.AddForeignKey(
                name: "fk_items_items_parent_item_container_guid",
                table: "items",
                column: "parent_item_container_guid",
                principalTable: "items",
                principalColumn: "guid",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
