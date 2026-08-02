using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserGroupTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_partitions_user_groups_user_group_guid",
                table: "partitions");

            migrationBuilder.DropIndex(
                name: "ix_partitions_user_group_guid",
                table: "partitions");

            migrationBuilder.DropColumn(
                name: "user_group_guid",
                table: "partitions");

            migrationBuilder.CreateTable(
                name: "user_group_partition_accesses",
                columns: table => new
                {
                    partition_accesses_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_group1guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_group_partition_accesses", x => new { x.partition_accesses_guid, x.user_group1guid });
                    table.ForeignKey(
                        name: "fk_user_group_partition_accesses_partitions_partition_accesses",
                        column: x => x.partition_accesses_guid,
                        principalTable: "partitions",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_group_partition_accesses_user_groups_user_group1guid",
                        column: x => x.user_group1guid,
                        principalTable: "user_groups",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_group_partition_accesses_user_group1guid",
                table: "user_group_partition_accesses",
                column: "user_group1guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_group_partition_accesses");

            migrationBuilder.AddColumn<Guid>(
                name: "user_group_guid",
                table: "partitions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_partitions_user_group_guid",
                table: "partitions",
                column: "user_group_guid");

            migrationBuilder.AddForeignKey(
                name: "fk_partitions_user_groups_user_group_guid",
                table: "partitions",
                column: "user_group_guid",
                principalTable: "user_groups",
                principalColumn: "guid");
        }
    }
}
