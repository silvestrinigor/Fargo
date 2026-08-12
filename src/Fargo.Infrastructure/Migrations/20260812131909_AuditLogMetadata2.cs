using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuditLogMetadata2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_article_partitions_partitions_partition_guid",
                table: "article_partitions");

            migrationBuilder.DropForeignKey(
                name: "fk_item_partitions_partitions_partition_guid",
                table: "item_partitions");

            migrationBuilder.DropForeignKey(
                name: "fk_user_group_partitions_partitions_partition_guid",
                table: "user_group_partitions");

            migrationBuilder.DropForeignKey(
                name: "fk_user_partitions_partitions_partition_guid",
                table: "user_partitions");

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
                name: "ix_audit_log_partitions_partition_guid",
                table: "audit_log_partitions",
                column: "partition_guid");

            migrationBuilder.AddForeignKey(
                name: "fk_article_partitions_partitions_partition_guid",
                table: "article_partitions",
                column: "partition_guid",
                principalTable: "partitions",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_item_partitions_partitions_partition_guid",
                table: "item_partitions",
                column: "partition_guid",
                principalTable: "partitions",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_group_partitions_partitions_partition_guid",
                table: "user_group_partitions",
                column: "partition_guid",
                principalTable: "partitions",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_partitions_partitions_partition_guid",
                table: "user_partitions",
                column: "partition_guid",
                principalTable: "partitions",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_article_partitions_partitions_partition_guid",
                table: "article_partitions");

            migrationBuilder.DropForeignKey(
                name: "fk_item_partitions_partitions_partition_guid",
                table: "item_partitions");

            migrationBuilder.DropForeignKey(
                name: "fk_user_group_partitions_partitions_partition_guid",
                table: "user_group_partitions");

            migrationBuilder.DropForeignKey(
                name: "fk_user_partitions_partitions_partition_guid",
                table: "user_partitions");

            migrationBuilder.DropTable(
                name: "audit_log_partitions");

            migrationBuilder.AddForeignKey(
                name: "fk_article_partitions_partitions_partition_guid",
                table: "article_partitions",
                column: "partition_guid",
                principalTable: "partitions",
                principalColumn: "guid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_item_partitions_partitions_partition_guid",
                table: "item_partitions",
                column: "partition_guid",
                principalTable: "partitions",
                principalColumn: "guid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_user_group_partitions_partitions_partition_guid",
                table: "user_group_partitions",
                column: "partition_guid",
                principalTable: "partitions",
                principalColumn: "guid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_user_partitions_partitions_partition_guid",
                table: "user_partitions",
                column: "partition_guid",
                principalTable: "partitions",
                principalColumn: "guid",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
