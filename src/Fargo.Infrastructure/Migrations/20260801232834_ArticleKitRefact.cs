using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ArticleKitRefact : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_article_kit_components_article_kits_kit_article_guid",
            table: "article_kit_components");

        migrationBuilder.DropTable(
            name: "article_kits");

        migrationBuilder.AddForeignKey(
            name: "fk_article_kit_components_articles_kit_article_guid",
            table: "article_kit_components",
            column: "kit_article_guid",
            principalTable: "articles",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_article_kit_components_articles_kit_article_guid",
            table: "article_kit_components");

        migrationBuilder.CreateTable(
            name: "article_kits",
            columns: table => new
            {
                kit_article_guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_article_kits", x => x.kit_article_guid);
                table.ForeignKey(
                    name: "fk_article_kits_articles_kit_article_guid",
                    column: x => x.kit_article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.AddForeignKey(
            name: "fk_article_kit_components_article_kits_kit_article_guid",
            table: "article_kit_components",
            column: "kit_article_guid",
            principalTable: "article_kits",
            principalColumn: "kit_article_guid",
            onDelete: ReferentialAction.Cascade);
    }
}
