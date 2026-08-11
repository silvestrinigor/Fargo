using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemParentContainerHistoryTableNameChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_item_moviments_items_item_guid",
                table: "item_moviments");

            migrationBuilder.DropForeignKey(
                name: "fk_item_moviments_items_item_guid1",
                table: "item_moviments");

            migrationBuilder.DropForeignKey(
                name: "fk_item_moviments_items_parent_item_contianer_guid",
                table: "item_moviments");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_item_moviments_item_guid_valid_at",
                table: "item_moviments");

            migrationBuilder.DropPrimaryKey(
                name: "pk_item_moviments",
                table: "item_moviments");

            migrationBuilder.RenameTable(
                name: "item_moviments",
                newName: "item_parent_containers_history");

            migrationBuilder.RenameIndex(
                name: "ix_item_moviments_parent_item_contianer_guid",
                table: "item_parent_containers_history",
                newName: "ix_item_parent_containers_history_parent_item_contianer_guid");

            migrationBuilder.RenameIndex(
                name: "ix_item_moviments_item_guid1",
                table: "item_parent_containers_history",
                newName: "ix_item_parent_containers_history_item_guid1");

            migrationBuilder.AddUniqueConstraint(
                name: "ak_item_parent_containers_history_item_guid_valid_at",
                table: "item_parent_containers_history",
                columns: new[] { "item_guid", "valid_at" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_item_parent_containers_history",
                table: "item_parent_containers_history",
                column: "guid");

            migrationBuilder.AddForeignKey(
                name: "fk_item_parent_containers_history_items_item_guid",
                table: "item_parent_containers_history",
                column: "item_guid",
                principalTable: "items",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_item_parent_containers_history_items_item_guid1",
                table: "item_parent_containers_history",
                column: "item_guid1",
                principalTable: "items",
                principalColumn: "guid");

            migrationBuilder.AddForeignKey(
                name: "fk_item_parent_containers_history_items_parent_item_contianer_",
                table: "item_parent_containers_history",
                column: "parent_item_contianer_guid",
                principalTable: "items",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_item_parent_containers_history_items_item_guid",
                table: "item_parent_containers_history");

            migrationBuilder.DropForeignKey(
                name: "fk_item_parent_containers_history_items_item_guid1",
                table: "item_parent_containers_history");

            migrationBuilder.DropForeignKey(
                name: "fk_item_parent_containers_history_items_parent_item_contianer_",
                table: "item_parent_containers_history");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_item_parent_containers_history_item_guid_valid_at",
                table: "item_parent_containers_history");

            migrationBuilder.DropPrimaryKey(
                name: "pk_item_parent_containers_history",
                table: "item_parent_containers_history");

            migrationBuilder.RenameTable(
                name: "item_parent_containers_history",
                newName: "item_moviments");

            migrationBuilder.RenameIndex(
                name: "ix_item_parent_containers_history_parent_item_contianer_guid",
                table: "item_moviments",
                newName: "ix_item_moviments_parent_item_contianer_guid");

            migrationBuilder.RenameIndex(
                name: "ix_item_parent_containers_history_item_guid1",
                table: "item_moviments",
                newName: "ix_item_moviments_item_guid1");

            migrationBuilder.AddUniqueConstraint(
                name: "ak_item_moviments_item_guid_valid_at",
                table: "item_moviments",
                columns: new[] { "item_guid", "valid_at" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_item_moviments",
                table: "item_moviments",
                column: "guid");

            migrationBuilder.AddForeignKey(
                name: "fk_item_moviments_items_item_guid",
                table: "item_moviments",
                column: "item_guid",
                principalTable: "items",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_item_moviments_items_item_guid1",
                table: "item_moviments",
                column: "item_guid1",
                principalTable: "items",
                principalColumn: "guid");

            migrationBuilder.AddForeignKey(
                name: "fk_item_moviments_items_parent_item_contianer_guid",
                table: "item_moviments",
                column: "parent_item_contianer_guid",
                principalTable: "items",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
