using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ArticleDimensionsSeparateTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "length_x",
            table: "articles");

        migrationBuilder.DropColumn(
            name: "length_y",
            table: "articles");

        migrationBuilder.DropColumn(
            name: "length_z",
            table: "articles");

        migrationBuilder.CreateTable(
            name: "article_dimensions",
            columns: table => new
            {
                article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                x = table.Column<string>(type: "text", nullable: true),
                y = table.Column<string>(type: "text", nullable: true),
                z = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_article_dimensions", x => x.article_guid);
                table.ForeignKey(
                    name: "fk_article_dimensions_articles_article_guid",
                    column: x => x.article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "article_dimensions");

        migrationBuilder.AddColumn<string>(
            name: "length_x",
            table: "articles",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "length_y",
            table: "articles",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "length_z",
            table: "articles",
            type: "text",
            nullable: true);
    }
}
