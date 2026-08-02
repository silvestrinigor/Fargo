using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewUserAuthTableAndGeneralRelationChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_article_kit_components_articles_from_article_guid",
                table: "article_kit_components");

            migrationBuilder.DropForeignKey(
                name: "fk_article_packs_articles_from_article_guid",
                table: "article_packs");

            migrationBuilder.DropForeignKey(
                name: "fk_article_variations_articles_from_article_guid",
                table: "article_variations");

            migrationBuilder.DropColumn(
                name: "auth_version",
                table: "users");

            migrationBuilder.DropColumn(
                name: "default_password_expiration_period",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_hash",
                table: "users");

            migrationBuilder.DropColumn(
                name: "require_password_change_at",
                table: "users");

            migrationBuilder.CreateTable(
                name: "user_authentications",
                columns: table => new
                {
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    default_password_expiration_period = table.Column<long>(type: "bigint", nullable: true),
                    require_password_change_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    auth_version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_authentications", x => x.user_guid);
                    table.ForeignKey(
                        name: "fk_user_authentications_users_user_guid",
                        column: x => x.user_guid,
                        principalTable: "users",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "fk_article_kit_components_articles_from_article_guid",
                table: "article_kit_components",
                column: "from_article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_article_packs_articles_from_article_guid",
                table: "article_packs",
                column: "from_article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_article_variations_articles_from_article_guid",
                table: "article_variations",
                column: "from_article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_article_kit_components_articles_from_article_guid",
                table: "article_kit_components");

            migrationBuilder.DropForeignKey(
                name: "fk_article_packs_articles_from_article_guid",
                table: "article_packs");

            migrationBuilder.DropForeignKey(
                name: "fk_article_variations_articles_from_article_guid",
                table: "article_variations");

            migrationBuilder.DropTable(
                name: "user_authentications");

            migrationBuilder.AddColumn<Guid>(
                name: "auth_version",
                table: "users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "default_password_expiration_period",
                table: "users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password_hash",
                table: "users",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "require_password_change_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_article_kit_components_articles_from_article_guid",
                table: "article_kit_components",
                column: "from_article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_article_packs_articles_from_article_guid",
                table: "article_packs",
                column: "from_article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_article_variations_articles_from_article_guid",
                table: "article_variations",
                column: "from_article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
