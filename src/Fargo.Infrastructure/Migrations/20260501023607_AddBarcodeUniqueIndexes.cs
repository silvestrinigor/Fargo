using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddBarcodeUniqueIndexes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Articles_Barcodes_Code128",
            table: "Articles",
            column: "Barcodes_Code128",
            unique: true,
            filter: "[Barcodes_Code128] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Barcodes_Code39",
            table: "Articles",
            column: "Barcodes_Code39",
            unique: true,
            filter: "[Barcodes_Code39] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Barcodes_DataMatrix",
            table: "Articles",
            column: "Barcodes_DataMatrix",
            unique: true,
            filter: "[Barcodes_DataMatrix] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Barcodes_Ean13",
            table: "Articles",
            column: "Barcodes_Ean13",
            unique: true,
            filter: "[Barcodes_Ean13] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Barcodes_Ean8",
            table: "Articles",
            column: "Barcodes_Ean8",
            unique: true,
            filter: "[Barcodes_Ean8] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Barcodes_Gs1128",
            table: "Articles",
            column: "Barcodes_Gs1128",
            unique: true,
            filter: "[Barcodes_Gs1128] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Barcodes_Itf14",
            table: "Articles",
            column: "Barcodes_Itf14",
            unique: true,
            filter: "[Barcodes_Itf14] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Barcodes_QrCode",
            table: "Articles",
            column: "Barcodes_QrCode",
            unique: true,
            filter: "[Barcodes_QrCode] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Barcodes_UpcA",
            table: "Articles",
            column: "Barcodes_UpcA",
            unique: true,
            filter: "[Barcodes_UpcA] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Barcodes_UpcE",
            table: "Articles",
            column: "Barcodes_UpcE",
            unique: true,
            filter: "[Barcodes_UpcE] IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Articles_Barcodes_Code128",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Barcodes_Code39",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Barcodes_DataMatrix",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Barcodes_Ean13",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Barcodes_Ean8",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Barcodes_Gs1128",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Barcodes_Itf14",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Barcodes_QrCode",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Barcodes_UpcA",
            table: "Articles");

        migrationBuilder.DropIndex(
            name: "IX_Articles_Barcodes_UpcE",
            table: "Articles");
    }
}
