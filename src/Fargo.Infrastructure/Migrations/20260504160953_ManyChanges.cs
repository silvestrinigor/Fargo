using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ManyChanges : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ApiClients")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ApiClientsHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<long>(
            name: "DefaultPasswordExpirationPeriod",
            table: "Users",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<long>(
            name: "DefaultPasswordExpirationPeriod",
            table: "Users",
            type: "bigint",
            nullable: false,
            defaultValue: 0L,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true);

        migrationBuilder.CreateTable(
            name: "ApiClients",
            columns: table => new
            {
                Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                EditedByGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                KeyHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApiClients", x => x.Guid);
            })
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "ApiClientsHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.CreateIndex(
            name: "IX_ApiClients_KeyHash",
            table: "ApiClients",
            column: "KeyHash",
            unique: true);
    }
}
