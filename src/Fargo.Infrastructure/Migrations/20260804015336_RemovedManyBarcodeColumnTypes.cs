using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class RemovedManyBarcodeColumnTypes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_article_barcodes_code128",
            table: "article_barcodes");

        migrationBuilder.DropIndex(
            name: "ix_article_barcodes_code39",
            table: "article_barcodes");

        migrationBuilder.DropIndex(
            name: "ix_article_barcodes_data_matrix",
            table: "article_barcodes");

        migrationBuilder.DropIndex(
            name: "ix_article_barcodes_ean8",
            table: "article_barcodes");

        migrationBuilder.DropIndex(
            name: "ix_article_barcodes_gs1128",
            table: "article_barcodes");

        migrationBuilder.DropIndex(
            name: "ix_article_barcodes_itf14",
            table: "article_barcodes");

        migrationBuilder.DropIndex(
            name: "ix_article_barcodes_qr_code",
            table: "article_barcodes");

        migrationBuilder.DropIndex(
            name: "ix_article_barcodes_upc_a",
            table: "article_barcodes");

        migrationBuilder.DropIndex(
            name: "ix_article_barcodes_upc_e",
            table: "article_barcodes");

        migrationBuilder.DropColumn(
            name: "code128",
            table: "article_barcodes");

        migrationBuilder.DropColumn(
            name: "code39",
            table: "article_barcodes");

        migrationBuilder.DropColumn(
            name: "data_matrix",
            table: "article_barcodes");

        migrationBuilder.DropColumn(
            name: "ean8",
            table: "article_barcodes");

        migrationBuilder.DropColumn(
            name: "gs1128",
            table: "article_barcodes");

        migrationBuilder.DropColumn(
            name: "itf14",
            table: "article_barcodes");

        migrationBuilder.DropColumn(
            name: "qr_code",
            table: "article_barcodes");

        migrationBuilder.DropColumn(
            name: "upc_a",
            table: "article_barcodes");

        migrationBuilder.DropColumn(
            name: "upc_e",
            table: "article_barcodes");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "code128",
            table: "article_barcodes",
            type: "character varying(80)",
            maxLength: 80,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "code39",
            table: "article_barcodes",
            type: "character varying(80)",
            maxLength: 80,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "data_matrix",
            table: "article_barcodes",
            type: "character varying(2335)",
            maxLength: 2335,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ean8",
            table: "article_barcodes",
            type: "character varying(8)",
            maxLength: 8,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "gs1128",
            table: "article_barcodes",
            type: "character varying(80)",
            maxLength: 80,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "itf14",
            table: "article_barcodes",
            type: "character varying(14)",
            maxLength: 14,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "qr_code",
            table: "article_barcodes",
            type: "character varying(2953)",
            maxLength: 2953,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "upc_a",
            table: "article_barcodes",
            type: "character varying(12)",
            maxLength: 12,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "upc_e",
            table: "article_barcodes",
            type: "character varying(8)",
            maxLength: 8,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_article_barcodes_code128",
            table: "article_barcodes",
            column: "code128",
            unique: true,
            filter: "code128 IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_article_barcodes_code39",
            table: "article_barcodes",
            column: "code39",
            unique: true,
            filter: "code39 IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_article_barcodes_data_matrix",
            table: "article_barcodes",
            column: "data_matrix",
            unique: true,
            filter: "data_matrix IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_article_barcodes_ean8",
            table: "article_barcodes",
            column: "ean8",
            unique: true,
            filter: "ean8 IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_article_barcodes_gs1128",
            table: "article_barcodes",
            column: "gs1128",
            unique: true,
            filter: "gs1128 IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_article_barcodes_itf14",
            table: "article_barcodes",
            column: "itf14",
            unique: true,
            filter: "itf14 IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_article_barcodes_qr_code",
            table: "article_barcodes",
            column: "qr_code",
            unique: true,
            filter: "qr_code IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_article_barcodes_upc_a",
            table: "article_barcodes",
            column: "upc_a",
            unique: true,
            filter: "upc_a IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_article_barcodes_upc_e",
            table: "article_barcodes",
            column: "upc_e",
            unique: true,
            filter: "upc_e IS NOT NULL");
    }
}
