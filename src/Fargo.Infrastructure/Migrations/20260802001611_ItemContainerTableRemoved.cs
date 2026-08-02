using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ItemContainerTableRemoved : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_items_item_containers_article_guid",
            table: "items");

        migrationBuilder.DropTable(
            name: "item_containers");

        migrationBuilder.DropIndex(
            name: "ix_items_article_guid1",
            table: "items");

        migrationBuilder.DropColumn(
            name: "article_guid1",
            table: "items");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "article_guid1",
            table: "items",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "item_containers",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_item_containers", x => x.guid);
            });

        migrationBuilder.CreateIndex(
            name: "ix_items_article_guid1",
            table: "items",
            column: "article_guid1",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "fk_items_item_containers_article_guid",
            table: "items",
            column: "article_guid1",
            principalTable: "item_containers",
            principalColumn: "guid",
            onDelete: ReferentialAction.Restrict);
    }
}
