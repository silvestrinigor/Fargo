using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class RefactArticleMetricsAndBarcodes2 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Articles_Code128",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Code39",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_DataMatrix",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Ean13",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Ean8",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Gs1128",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Itf14",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_QrCode",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_UpcA",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_UpcE",
            table: "Articles");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Code128",
            table: "Articles",
            column: "Code128",
            unique: true,
            filter: "[Code128] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Code39",
            table: "Articles",
            column: "Code39",
            unique: true,
            filter: "[Code39] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_DataMatrix",
            table: "Articles",
            column: "DataMatrix",
            unique: true,
            filter: "[DataMatrix] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Ean13",
            table: "Articles",
            column: "Ean13",
            unique: true,
            filter: "[Ean13] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Ean8",
            table: "Articles",
            column: "Ean8",
            unique: true,
            filter: "[Ean8] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Gs1128",
            table: "Articles",
            column: "Gs1128",
            unique: true,
            filter: "[Gs1128] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Itf14",
            table: "Articles",
            column: "Itf14",
            unique: true,
            filter: "[Itf14] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_QrCode",
            table: "Articles",
            column: "QrCode",
            unique: true,
            filter: "[QrCode] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_UpcA",
            table: "Articles",
            column: "UpcA",
            unique: true,
            filter: "[UpcA] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_UpcE",
            table: "Articles",
            column: "UpcE",
            unique: true,
            filter: "[UpcE] IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Articles_Code128",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Code39",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_DataMatrix",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Ean13",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Ean8",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Gs1128",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Itf14",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_QrCode",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_UpcA",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_UpcE",
            table: "Articles");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Code128",
            table: "Articles",
            column: "Code128",
            unique: true,
            filter: "[Code128] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Code39",
            table: "Articles",
            column: "Code39",
            unique: true,
            filter: "[Code39] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_DataMatrix",
            table: "Articles",
            column: "DataMatrix",
            unique: true,
            filter: "[DataMatrix] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Ean13",
            table: "Articles",
            column: "Ean13",
            unique: true,
            filter: "[Ean13] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Ean8",
            table: "Articles",
            column: "Ean8",
            unique: true,
            filter: "[Ean8] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Gs1128",
            table: "Articles",
            column: "Gs1128",
            unique: true,
            filter: "[Gs1128] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Itf14",
            table: "Articles",
            column: "Itf14",
            unique: true,
            filter: "[Itf14] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_QrCode",
            table: "Articles",
            column: "QrCode",
            unique: true,
            filter: "[QrCode] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_UpcA",
            table: "Articles",
            column: "UpcA",
            unique: true,
            filter: "[UpcA] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_UpcE",
            table: "Articles",
            column: "UpcE",
            unique: true,
            filter: "[UpcE] IS NOT NULL");
    }
}
