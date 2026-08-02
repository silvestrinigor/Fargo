using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ArticleBarcodesSeparateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_articles_code128",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_articles_code39",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_articles_data_matrix",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_articles_ean13",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_articles_ean8",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_articles_gs1128",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_articles_itf14",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_articles_qr_code",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_articles_upc_a",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_articles_upc_e",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "code128",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "code39",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "data_matrix",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "ean13",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "ean8",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "gs1128",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "itf14",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "qr_code",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "upc_a",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "upc_e",
                table: "articles");

            migrationBuilder.CreateTable(
                name: "article_barcodes",
                columns: table => new
                {
                    article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    ean13 = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    ean8 = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    upc_a = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    upc_e = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    code128 = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    code39 = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    itf14 = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    gs1128 = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    qr_code = table.Column<string>(type: "character varying(2953)", maxLength: 2953, nullable: true),
                    data_matrix = table.Column<string>(type: "character varying(2335)", maxLength: 2335, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_barcodes", x => x.article_guid);
                    table.ForeignKey(
                        name: "fk_article_barcodes_articles_article_guid",
                        column: x => x.article_guid,
                        principalTable: "articles",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "ix_article_barcodes_ean13",
                table: "article_barcodes",
                column: "ean13",
                unique: true,
                filter: "ean13 IS NOT NULL");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_barcodes");

            migrationBuilder.AddColumn<string>(
                name: "code128",
                table: "articles",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "code39",
                table: "articles",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "data_matrix",
                table: "articles",
                type: "character varying(2335)",
                maxLength: 2335,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ean13",
                table: "articles",
                type: "character varying(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ean8",
                table: "articles",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "gs1128",
                table: "articles",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "itf14",
                table: "articles",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "qr_code",
                table: "articles",
                type: "character varying(2953)",
                maxLength: 2953,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "upc_a",
                table: "articles",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "upc_e",
                table: "articles",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_articles_code128",
                table: "articles",
                column: "code128",
                unique: true,
                filter: "code128 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_code39",
                table: "articles",
                column: "code39",
                unique: true,
                filter: "code39 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_data_matrix",
                table: "articles",
                column: "data_matrix",
                unique: true,
                filter: "data_matrix IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_ean13",
                table: "articles",
                column: "ean13",
                unique: true,
                filter: "ean13 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_ean8",
                table: "articles",
                column: "ean8",
                unique: true,
                filter: "ean8 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_gs1128",
                table: "articles",
                column: "gs1128",
                unique: true,
                filter: "gs1128 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_itf14",
                table: "articles",
                column: "itf14",
                unique: true,
                filter: "itf14 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_qr_code",
                table: "articles",
                column: "qr_code",
                unique: true,
                filter: "qr_code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_upc_a",
                table: "articles",
                column: "upc_a",
                unique: true,
                filter: "upc_a IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_upc_e",
                table: "articles",
                column: "upc_e",
                unique: true,
                filter: "upc_e IS NOT NULL");
        }
    }
}
