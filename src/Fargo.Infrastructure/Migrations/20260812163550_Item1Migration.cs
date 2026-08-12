using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Item1Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "ak_item_parent_containers_history_item_guid_valid_at",
                table: "item_parent_containers_history");

            migrationBuilder.CreateIndex(
                name: "ix_item_parent_containers_history_item_guid",
                table: "item_parent_containers_history",
                column: "item_guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_item_parent_containers_history_item_guid",
                table: "item_parent_containers_history");

            migrationBuilder.AddUniqueConstraint(
                name: "ak_item_parent_containers_history_item_guid_valid_at",
                table: "item_parent_containers_history",
                columns: new[] { "item_guid", "valid_at" });
        }
    }
}
