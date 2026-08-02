using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserKitPackVariationKeyRefact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_article_kit_components_article_kits_article_kit_guid",
                table: "article_kit_components");

            migrationBuilder.DropForeignKey(
                name: "fk_article_kit_components_articles_article_guid",
                table: "article_kit_components");

            migrationBuilder.DropForeignKey(
                name: "fk_article_packs_articles_from_article_guid",
                table: "article_packs");

            migrationBuilder.DropForeignKey(
                name: "fk_article_variations_articles_from_article_guid",
                table: "article_variations");

            migrationBuilder.DropForeignKey(
                name: "fk_articles_article_kits_article_guid",
                table: "articles");

            migrationBuilder.DropForeignKey(
                name: "fk_articles_article_packs_pack_guid",
                table: "articles");

            migrationBuilder.DropForeignKey(
                name: "fk_articles_article_variations_variation_guid",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_articles_article_guid",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_articles_pack_guid",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_articles_variation_guid",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_article_kit_components_article_kit_guid",
                table: "article_kit_components");

            migrationBuilder.DropColumn(
                name: "article_guid",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "pack_guid",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "variation_guid",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "article_kit_guid",
                table: "article_kit_components");

            migrationBuilder.RenameColumn(
                name: "guid",
                table: "article_variations",
                newName: "variation_article_guid");

            migrationBuilder.RenameColumn(
                name: "guid",
                table: "article_packs",
                newName: "pack_article_guid");

            migrationBuilder.RenameColumn(
                name: "guid",
                table: "article_kits",
                newName: "kit_article_guid");

            migrationBuilder.RenameColumn(
                name: "article_guid",
                table: "article_kit_components",
                newName: "from_article_guid");

            migrationBuilder.RenameColumn(
                name: "guid",
                table: "article_kit_components",
                newName: "kit_article_guid");

            migrationBuilder.RenameIndex(
                name: "ix_article_kit_components_article_guid",
                table: "article_kit_components",
                newName: "ix_article_kit_components_from_article_guid");

            migrationBuilder.AddForeignKey(
                name: "fk_article_kit_components_article_kits_kit_article_guid",
                table: "article_kit_components",
                column: "kit_article_guid",
                principalTable: "article_kits",
                principalColumn: "kit_article_guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_article_kit_components_articles_from_article_guid",
                table: "article_kit_components",
                column: "from_article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_article_kits_articles_kit_article_guid",
                table: "article_kits",
                column: "kit_article_guid",
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
                name: "fk_article_packs_articles_pack_article_guid",
                table: "article_packs",
                column: "pack_article_guid",
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

            migrationBuilder.AddForeignKey(
                name: "fk_article_variations_articles_variation_article_guid",
                table: "article_variations",
                column: "variation_article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_article_kit_components_article_kits_kit_article_guid",
                table: "article_kit_components");

            migrationBuilder.DropForeignKey(
                name: "fk_article_kit_components_articles_from_article_guid",
                table: "article_kit_components");

            migrationBuilder.DropForeignKey(
                name: "fk_article_kits_articles_kit_article_guid",
                table: "article_kits");

            migrationBuilder.DropForeignKey(
                name: "fk_article_packs_articles_from_article_guid",
                table: "article_packs");

            migrationBuilder.DropForeignKey(
                name: "fk_article_packs_articles_pack_article_guid",
                table: "article_packs");

            migrationBuilder.DropForeignKey(
                name: "fk_article_variations_articles_from_article_guid",
                table: "article_variations");

            migrationBuilder.DropForeignKey(
                name: "fk_article_variations_articles_variation_article_guid",
                table: "article_variations");

            migrationBuilder.RenameColumn(
                name: "variation_article_guid",
                table: "article_variations",
                newName: "guid");

            migrationBuilder.RenameColumn(
                name: "pack_article_guid",
                table: "article_packs",
                newName: "guid");

            migrationBuilder.RenameColumn(
                name: "kit_article_guid",
                table: "article_kits",
                newName: "guid");

            migrationBuilder.RenameColumn(
                name: "from_article_guid",
                table: "article_kit_components",
                newName: "article_guid");

            migrationBuilder.RenameColumn(
                name: "kit_article_guid",
                table: "article_kit_components",
                newName: "guid");

            migrationBuilder.RenameIndex(
                name: "ix_article_kit_components_from_article_guid",
                table: "article_kit_components",
                newName: "ix_article_kit_components_article_guid");

            migrationBuilder.AddColumn<Guid>(
                name: "article_guid",
                table: "articles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "pack_guid",
                table: "articles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "variation_guid",
                table: "articles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "article_kit_guid",
                table: "article_kit_components",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_articles_article_guid",
                table: "articles",
                column: "article_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_articles_pack_guid",
                table: "articles",
                column: "pack_guid");

            migrationBuilder.CreateIndex(
                name: "ix_articles_variation_guid",
                table: "articles",
                column: "variation_guid");

            migrationBuilder.CreateIndex(
                name: "ix_article_kit_components_article_kit_guid",
                table: "article_kit_components",
                column: "article_kit_guid");

            migrationBuilder.AddForeignKey(
                name: "fk_article_kit_components_article_kits_article_kit_guid",
                table: "article_kit_components",
                column: "article_kit_guid",
                principalTable: "article_kits",
                principalColumn: "guid");

            migrationBuilder.AddForeignKey(
                name: "fk_article_kit_components_articles_article_guid",
                table: "article_kit_components",
                column: "article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "fk_articles_article_kits_article_guid",
                table: "articles",
                column: "article_guid",
                principalTable: "article_kits",
                principalColumn: "guid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_articles_article_packs_pack_guid",
                table: "articles",
                column: "pack_guid",
                principalTable: "article_packs",
                principalColumn: "guid");

            migrationBuilder.AddForeignKey(
                name: "fk_articles_article_variations_variation_guid",
                table: "articles",
                column: "variation_guid",
                principalTable: "article_variations",
                principalColumn: "guid");
        }
    }
}
