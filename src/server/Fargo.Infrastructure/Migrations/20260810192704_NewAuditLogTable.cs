using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class NewAuditLogTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "audit_logs",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                actor_guid = table.Column<Guid>(type: "uuid", nullable: false),
                actor_type = table.Column<byte>(type: "smallint", nullable: false),
                action_type = table.Column<int>(type: "integer", nullable: false),
                entity_guid = table.Column<Guid>(type: "uuid", nullable: false),
                entity_type = table.Column<int>(type: "integer", nullable: false),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_audit_logs", x => x.guid);
            });

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_occurred_at",
            table: "audit_logs",
            column: "occurred_at");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "audit_logs");
    }
}
