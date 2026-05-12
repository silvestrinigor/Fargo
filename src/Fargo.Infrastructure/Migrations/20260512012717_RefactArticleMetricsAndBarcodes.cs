using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactArticleMetricsAndBarcodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Barcodes_UpcE",
                table: "Articles",
                newName: "UpcE");

            migrationBuilder.RenameColumn(
                name: "Barcodes_UpcA",
                table: "Articles",
                newName: "UpcA");

            migrationBuilder.RenameColumn(
                name: "Barcodes_QrCode",
                table: "Articles",
                newName: "QrCode");

            migrationBuilder.RenameColumn(
                name: "Barcodes_Itf14",
                table: "Articles",
                newName: "Itf14");

            migrationBuilder.RenameColumn(
                name: "Barcodes_Gs1128",
                table: "Articles",
                newName: "Gs1128");

            migrationBuilder.RenameColumn(
                name: "Barcodes_Ean8",
                table: "Articles",
                newName: "Ean8");

            migrationBuilder.RenameColumn(
                name: "Barcodes_Ean13",
                table: "Articles",
                newName: "Ean13");

            migrationBuilder.RenameColumn(
                name: "Barcodes_DataMatrix",
                table: "Articles",
                newName: "DataMatrix");

            migrationBuilder.RenameColumn(
                name: "Barcodes_Code39",
                table: "Articles",
                newName: "Code39");

            migrationBuilder.RenameColumn(
                name: "Barcodes_Code128",
                table: "Articles",
                newName: "Code128");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Barcodes_UpcE",
                table: "Articles",
                newName: "IX_Articles_UpcE");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Barcodes_UpcA",
                table: "Articles",
                newName: "IX_Articles_UpcA");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Barcodes_QrCode",
                table: "Articles",
                newName: "IX_Articles_QrCode");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Barcodes_Itf14",
                table: "Articles",
                newName: "IX_Articles_Itf14");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Barcodes_Gs1128",
                table: "Articles",
                newName: "IX_Articles_Gs1128");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Barcodes_Ean8",
                table: "Articles",
                newName: "IX_Articles_Ean8");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Barcodes_Ean13",
                table: "Articles",
                newName: "IX_Articles_Ean13");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Barcodes_DataMatrix",
                table: "Articles",
                newName: "IX_Articles_DataMatrix");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Barcodes_Code39",
                table: "Articles",
                newName: "IX_Articles_Code39");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Barcodes_Code128",
                table: "Articles",
                newName: "IX_Articles_Code128");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpcE",
                table: "Articles",
                newName: "Barcodes_UpcE");

            migrationBuilder.RenameColumn(
                name: "UpcA",
                table: "Articles",
                newName: "Barcodes_UpcA");

            migrationBuilder.RenameColumn(
                name: "QrCode",
                table: "Articles",
                newName: "Barcodes_QrCode");

            migrationBuilder.RenameColumn(
                name: "Itf14",
                table: "Articles",
                newName: "Barcodes_Itf14");

            migrationBuilder.RenameColumn(
                name: "Gs1128",
                table: "Articles",
                newName: "Barcodes_Gs1128");

            migrationBuilder.RenameColumn(
                name: "Ean8",
                table: "Articles",
                newName: "Barcodes_Ean8");

            migrationBuilder.RenameColumn(
                name: "Ean13",
                table: "Articles",
                newName: "Barcodes_Ean13");

            migrationBuilder.RenameColumn(
                name: "DataMatrix",
                table: "Articles",
                newName: "Barcodes_DataMatrix");

            migrationBuilder.RenameColumn(
                name: "Code39",
                table: "Articles",
                newName: "Barcodes_Code39");

            migrationBuilder.RenameColumn(
                name: "Code128",
                table: "Articles",
                newName: "Barcodes_Code128");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_UpcE",
                table: "Articles",
                newName: "IX_Articles_Barcodes_UpcE");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_UpcA",
                table: "Articles",
                newName: "IX_Articles_Barcodes_UpcA");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_QrCode",
                table: "Articles",
                newName: "IX_Articles_Barcodes_QrCode");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Itf14",
                table: "Articles",
                newName: "IX_Articles_Barcodes_Itf14");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Gs1128",
                table: "Articles",
                newName: "IX_Articles_Barcodes_Gs1128");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Ean8",
                table: "Articles",
                newName: "IX_Articles_Barcodes_Ean8");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Ean13",
                table: "Articles",
                newName: "IX_Articles_Barcodes_Ean13");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_DataMatrix",
                table: "Articles",
                newName: "IX_Articles_Barcodes_DataMatrix");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Code39",
                table: "Articles",
                newName: "IX_Articles_Barcodes_Code39");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_Code128",
                table: "Articles",
                newName: "IX_Articles_Barcodes_Code128");
        }
    }
}
