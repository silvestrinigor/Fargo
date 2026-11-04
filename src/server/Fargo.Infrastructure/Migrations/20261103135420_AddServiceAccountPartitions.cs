using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddServiceAccountPartitions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "service_account_partitions",
            columns: table => new
            {
                service_account_guid = table.Column<Guid>(type: "uuid", nullable: false),
                partition_guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_service_account_partitions", x => new { x.service_account_guid, x.partition_guid });
                table.ForeignKey(
                    name: "fk_service_account_partitions_partitions_partition_guid",
                    column: x => x.partition_guid,
                    principalTable: "partitions",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_service_account_partitions_service_accounts_service_account",
                    column: x => x.service_account_guid,
                    principalTable: "service_accounts",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_service_account_partitions_partition_guid",
            table: "service_account_partitions",
            column: "partition_guid");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "service_account_partitions");
    }
}
