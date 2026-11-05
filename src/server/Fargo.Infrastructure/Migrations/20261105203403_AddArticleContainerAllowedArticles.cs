using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleContainerAllowedArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "article_container_allowed_articles",
                columns: table => new
                {
                    container_article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    allowed_article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    max_quantity = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_container_allowed_articles", x => new { x.container_article_guid, x.allowed_article_guid });
                    table.ForeignKey(
                        name: "fk_article_container_allowed_articles_article_containers_conta",
                        column: x => x.container_article_guid,
                        principalTable: "article_containers",
                        principalColumn: "article_guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_article_container_allowed_articles_articles_allowed_article",
                        column: x => x.allowed_article_guid,
                        principalTable: "articles",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_article_container_allowed_articles_allowed_article_guid",
                table: "article_container_allowed_articles",
                column: "allowed_article_guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_container_allowed_articles");
        }
    }
}
