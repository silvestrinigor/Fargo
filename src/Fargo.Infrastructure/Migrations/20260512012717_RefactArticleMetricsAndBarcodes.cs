using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactArticleMetricsAndBarcodes : Migration
    {
        private static readonly (string OldName, string NewName)[] BarcodeColumns =
        {
            ("Barcodes_UpcE", "UpcE"),
            ("Barcodes_UpcA", "UpcA"),
            ("Barcodes_QrCode", "QrCode"),
            ("Barcodes_Itf14", "Itf14"),
            ("Barcodes_Gs1128", "Gs1128"),
            ("Barcodes_Ean8", "Ean8"),
            ("Barcodes_Ean13", "Ean13"),
            ("Barcodes_DataMatrix", "DataMatrix"),
            ("Barcodes_Code39", "Code39"),
            ("Barcodes_Code128", "Code128")
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            DropBarcodeIndexes(migrationBuilder, useOldNames: true);
            RenameBarcodeColumns(migrationBuilder, oldToNew: true);
            CreateBarcodeIndexes(migrationBuilder, useOldNames: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropBarcodeIndexes(migrationBuilder, useOldNames: false);
            RenameBarcodeColumns(migrationBuilder, oldToNew: false);
            CreateBarcodeIndexes(migrationBuilder, useOldNames: true);
        }

        private static void DropBarcodeIndexes(MigrationBuilder migrationBuilder, bool useOldNames)
        {
            foreach (var (oldName, newName) in BarcodeColumns)
            {
                var columnName = useOldNames ? oldName : newName;
                migrationBuilder.DropIndex(
                    name: $"IX_Articles_{columnName}",
                    table: "Articles");
            }
        }

        private static void RenameBarcodeColumns(MigrationBuilder migrationBuilder, bool oldToNew)
        {
            foreach (var (oldName, newName) in BarcodeColumns)
            {
                migrationBuilder.RenameColumn(
                    name: oldToNew ? oldName : newName,
                    table: "Articles",
                    newName: oldToNew ? newName : oldName);
            }
        }

        private static void CreateBarcodeIndexes(MigrationBuilder migrationBuilder, bool useOldNames)
        {
            foreach (var (oldName, newName) in BarcodeColumns)
            {
                var columnName = useOldNames ? oldName : newName;
                migrationBuilder.CreateIndex(
                    name: $"IX_Articles_{columnName}",
                    table: "Articles",
                    column: columnName,
                    unique: true,
                    filter: $"[{columnName}] IS NOT NULL");
            }
        }
    }
}
