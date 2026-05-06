using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ItemContainersAndArticleRelations : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "ParentContainerGuid",
            table: "Items",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "ArticleContainers",
            columns: table => new
            {
                ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MaxMass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticleContainers", x => x.ArticleGuid);
                table.ForeignKey(
                    name: "FK_ArticleContainers_Articles_ArticleGuid",
                    column: x => x.ArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ArticleKits",
            columns: table => new
            {
                ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticleKits", x => x.ArticleGuid);
                table.ForeignKey(
                    name: "FK_ArticleKits_Articles_ArticleGuid",
                    column: x => x.ArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ArticlePacks",
            columns: table => new
            {
                ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FromArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Quantity = table.Column<double>(type: "float", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticlePacks", x => x.ArticleGuid);
                table.ForeignKey(
                    name: "FK_ArticlePacks_Articles_ArticleGuid",
                    column: x => x.ArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ArticlePacks_Articles_FromArticleGuid",
                    column: x => x.FromArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ArticleVariations",
            columns: table => new
            {
                ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FromArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticleVariations", x => x.ArticleGuid);
                table.ForeignKey(
                    name: "FK_ArticleVariations_Articles_ArticleGuid",
                    column: x => x.ArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ArticleVariations_Articles_FromArticleGuid",
                    column: x => x.FromArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ArticleKitPacks",
            columns: table => new
            {
                Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FromArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Quantity = table.Column<double>(type: "float", nullable: false),
                KitArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticleKitPacks", x => x.Guid);
                table.ForeignKey(
                    name: "FK_ArticleKitPacks_ArticleKits_KitArticleGuid",
                    column: x => x.KitArticleGuid,
                    principalTable: "ArticleKits",
                    principalColumn: "ArticleGuid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ArticleKitPacks_Articles_FromArticleGuid",
                    column: x => x.FromArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Items_ParentContainerGuid",
            table: "Items",
            column: "ParentContainerGuid");

        migrationBuilder.CreateIndex(
            name: "IX_ArticleKitPacks_FromArticleGuid",
            table: "ArticleKitPacks",
            column: "FromArticleGuid");

        migrationBuilder.CreateIndex(
            name: "IX_ArticleKitPacks_KitArticleGuid_FromArticleGuid",
            table: "ArticleKitPacks",
            columns: new[] { "KitArticleGuid", "FromArticleGuid" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ArticlePacks_FromArticleGuid",
            table: "ArticlePacks",
            column: "FromArticleGuid");

        migrationBuilder.CreateIndex(
            name: "IX_ArticleVariations_FromArticleGuid",
            table: "ArticleVariations",
            column: "FromArticleGuid");

        migrationBuilder.AddForeignKey(
            name: "FK_Items_Items_ParentContainerGuid",
            table: "Items",
            column: "ParentContainerGuid",
            principalTable: "Items",
            principalColumn: "Guid",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Items_Items_ParentContainerGuid",
            table: "Items");

        migrationBuilder.DropTable(
            name: "ArticleContainers");

        migrationBuilder.DropTable(
            name: "ArticleKitPacks");

        migrationBuilder.DropTable(
            name: "ArticlePacks");

        migrationBuilder.DropTable(
            name: "ArticleVariations");

        migrationBuilder.DropTable(
            name: "ArticleKits");

        migrationBuilder.DropIndex(
            name: "IX_Items_ParentContainerGuid",
            table: "Items");

        migrationBuilder.DropColumn(
            name: "ParentContainerGuid",
            table: "Items");
    }
}
