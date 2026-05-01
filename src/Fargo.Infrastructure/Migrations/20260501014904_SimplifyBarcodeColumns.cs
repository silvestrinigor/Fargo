using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class SimplifyBarcodeColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ArticleCode128")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleCode128History")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.DropTable(
            name: "ArticleCode39")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleCode39History")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.DropTable(
            name: "ArticleDataMatrix")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleDataMatrixHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.DropTable(
            name: "ArticleEan13")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleEan13History")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.DropTable(
            name: "ArticleEan8")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleEan8History")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.DropTable(
            name: "ArticleGs1128")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleGs1128History")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.DropTable(
            name: "ArticleItf14")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleItf14History")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.DropTable(
            name: "ArticleQrCode")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleQrCodeHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.DropTable(
            name: "ArticleUpcA")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleUpcAHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.DropTable(
            name: "ArticleUpcE")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleUpcEHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AddColumn<string>(
            name: "Barcodes_Code128",
            table: "Articles",
            type: "nvarchar(80)",
            maxLength: 80,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Barcodes_Code39",
            table: "Articles",
            type: "nvarchar(80)",
            maxLength: 80,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Barcodes_DataMatrix",
            table: "Articles",
            type: "nvarchar(2335)",
            maxLength: 2335,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Barcodes_Ean13",
            table: "Articles",
            type: "nvarchar(13)",
            maxLength: 13,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Barcodes_Ean8",
            table: "Articles",
            type: "nvarchar(8)",
            maxLength: 8,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Barcodes_Gs1128",
            table: "Articles",
            type: "nvarchar(80)",
            maxLength: 80,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Barcodes_Itf14",
            table: "Articles",
            type: "nvarchar(14)",
            maxLength: 14,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Barcodes_QrCode",
            table: "Articles",
            type: "nvarchar(2953)",
            maxLength: 2953,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Barcodes_UpcA",
            table: "Articles",
            type: "nvarchar(12)",
            maxLength: 12,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Barcodes_UpcE",
            table: "Articles",
            type: "nvarchar(8)",
            maxLength: 8,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Barcodes_Code128",
            table: "Articles");

        migrationBuilder.DropColumn(
            name: "Barcodes_Code39",
            table: "Articles");

        migrationBuilder.DropColumn(
            name: "Barcodes_DataMatrix",
            table: "Articles");

        migrationBuilder.DropColumn(
            name: "Barcodes_Ean13",
            table: "Articles");

        migrationBuilder.DropColumn(
            name: "Barcodes_Ean8",
            table: "Articles");

        migrationBuilder.DropColumn(
            name: "Barcodes_Gs1128",
            table: "Articles");

        migrationBuilder.DropColumn(
            name: "Barcodes_Itf14",
            table: "Articles");

        migrationBuilder.DropColumn(
            name: "Barcodes_QrCode",
            table: "Articles");

        migrationBuilder.DropColumn(
            name: "Barcodes_UpcA",
            table: "Articles");

        migrationBuilder.DropColumn(
            name: "Barcodes_UpcE",
            table: "Articles");

        migrationBuilder.CreateTable(
            name: "ArticleCode128",
            columns: table => new
            {
                ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticleCode128", x => x.ArticleGuid);
                table.ForeignKey(
                    name: "FK_ArticleCode128_Articles_ArticleGuid",
                    column: x => x.ArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleCode128History")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.CreateTable(
            name: "ArticleCode39",
            columns: table => new
            {
                ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticleCode39", x => x.ArticleGuid);
                table.ForeignKey(
                    name: "FK_ArticleCode39_Articles_ArticleGuid",
                    column: x => x.ArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleCode39History")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.CreateTable(
            name: "ArticleDataMatrix",
            columns: table => new
            {
                ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(2335)", maxLength: 2335, nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticleDataMatrix", x => x.ArticleGuid);
                table.ForeignKey(
                    name: "FK_ArticleDataMatrix_Articles_ArticleGuid",
                    column: x => x.ArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleDataMatrixHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.CreateTable(
            name: "ArticleEan13",
            columns: table => new
            {
                ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticleEan13", x => x.ArticleGuid);
                table.ForeignKey(
                    name: "FK_ArticleEan13_Articles_ArticleGuid",
                    column: x => x.ArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleEan13History")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.CreateTable(
            name: "ArticleEan8",
            columns: table => new
            {
                ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticleEan8", x => x.ArticleGuid);
                table.ForeignKey(
                    name: "FK_ArticleEan8_Articles_ArticleGuid",
                    column: x => x.ArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleEan8History")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.CreateTable(
            name: "ArticleGs1128",
            columns: table => new
            {
                ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticleGs1128", x => x.ArticleGuid);
                table.ForeignKey(
                    name: "FK_ArticleGs1128_Articles_ArticleGuid",
                    column: x => x.ArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleGs1128History")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.CreateTable(
            name: "ArticleItf14",
            columns: table => new
            {
                ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticleItf14", x => x.ArticleGuid);
                table.ForeignKey(
                    name: "FK_ArticleItf14_Articles_ArticleGuid",
                    column: x => x.ArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleItf14History")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.CreateTable(
            name: "ArticleQrCode",
            columns: table => new
            {
                ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(2953)", maxLength: 2953, nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticleQrCode", x => x.ArticleGuid);
                table.ForeignKey(
                    name: "FK_ArticleQrCode_Articles_ArticleGuid",
                    column: x => x.ArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleQrCodeHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.CreateTable(
            name: "ArticleUpcA",
            columns: table => new
            {
                ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticleUpcA", x => x.ArticleGuid);
                table.ForeignKey(
                    name: "FK_ArticleUpcA_Articles_ArticleGuid",
                    column: x => x.ArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleUpcAHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.CreateTable(
            name: "ArticleUpcE",
            columns: table => new
            {
                ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticleUpcE", x => x.ArticleGuid);
                table.ForeignKey(
                    name: "FK_ArticleUpcE_Articles_ArticleGuid",
                    column: x => x.ArticleGuid,
                    principalTable: "Articles",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ArticleUpcEHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");
    }
}
