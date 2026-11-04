using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CommandBasedAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log_partitions");

            migrationBuilder.DropIndex(
                name: "ix_audit_logs_entity_guid_occurred_at",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "entity_guid",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "entity_type",
                table: "audit_logs");

            migrationBuilder.AddColumn<string>(
                name: "command_name",
                table: "audit_logs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "fk_audit_logs_actors_actor_guid",
                table: "audit_logs",
                column: "actor_guid",
                principalTable: "actors",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_audit_logs_actors_actor_guid",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "command_name",
                table: "audit_logs");

            migrationBuilder.AddColumn<Guid>(
                name: "entity_guid",
                table: "audit_logs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "entity_type",
                table: "audit_logs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "audit_log_partitions",
                columns: table => new
                {
                    audit_log_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    partition_guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log_partitions", x => new { x.audit_log_guid, x.partition_guid });
                    table.ForeignKey(
                        name: "fk_audit_log_partitions_audit_logs_audit_log_guid",
                        column: x => x.audit_log_guid,
                        principalTable: "audit_logs",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_audit_log_partitions_partitions_partition_guid",
                        column: x => x.partition_guid,
                        principalTable: "partitions",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity_guid_occurred_at",
                table: "audit_logs",
                columns: new[] { "entity_guid", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_partitions_partition_guid",
                table: "audit_log_partitions",
                column: "partition_guid");
        }
    }
}
