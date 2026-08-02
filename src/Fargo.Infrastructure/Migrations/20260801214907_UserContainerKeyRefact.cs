using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserContainerKeyRefact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_articles_article_containers_article_guid",
                table: "articles");

            migrationBuilder.RenameColumn(
                name: "guid",
                table: "article_containers",
                newName: "article_guid");

            migrationBuilder.AddForeignKey(
                name: "fk_article_containers_articles_article_guid",
                table: "article_containers",
                column: "article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_article_containers_articles_article_guid",
                table: "article_containers");

            migrationBuilder.RenameColumn(
                name: "article_guid",
                table: "article_containers",
                newName: "guid");

            migrationBuilder.AddForeignKey(
                name: "fk_articles_article_containers_article_guid",
                table: "articles",
                column: "article_guid",
                principalTable: "article_containers",
                principalColumn: "guid",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
