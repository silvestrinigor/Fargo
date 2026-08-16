using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewMigration67 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_actor_guid_occurred_at",
                table: "audit_logs",
                columns: new[] { "actor_guid", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity_guid_occurred_at",
                table: "audit_logs",
                columns: new[] { "entity_guid", "occurred_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_audit_logs_actor_guid_occurred_at",
                table: "audit_logs");

            migrationBuilder.DropIndex(
                name: "ix_audit_logs_entity_guid_occurred_at",
                table: "audit_logs");
        }
    }
}
