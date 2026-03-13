using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Partition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Partitions",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ParentPartitionGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsGlobal = table.Column<bool>(type: "bit", nullable: false),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false),
                    ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ItemGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true),
                    UserGroupGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EditedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EditedByGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partitions", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_Partitions_Articles_ArticleGuid",
                        column: x => x.ArticleGuid,
                        principalTable: "Articles",
                        principalColumn: "Guid");
                    table.ForeignKey(
                        name: "FK_Partitions_Items_ItemGuid",
                        column: x => x.ItemGuid,
                        principalTable: "Items",
                        principalColumn: "Guid");
                    table.ForeignKey(
                        name: "FK_Partitions_Partitions_ParentPartitionGuid",
                        column: x => x.ParentPartitionGuid,
                        principalTable: "Partitions",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partitions_UserGroups_UserGroupGuid",
                        column: x => x.UserGroupGuid,
                        principalTable: "UserGroups",
                        principalColumn: "Guid");
                    table.ForeignKey(
                        name: "FK_Partitions_Users_UserGuid",
                        column: x => x.UserGuid,
                        principalTable: "Users",
                        principalColumn: "Guid");
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PartitionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateTable(
                name: "PartitionAccesses",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartitionGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartitionAccesses", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_PartitionAccesses_Partitions_PartitionGuid",
                        column: x => x.PartitionGuid,
                        principalTable: "Partitions",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartitionAccesses_Users_UserGuid",
                        column: x => x.UserGuid,
                        principalTable: "Users",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PartitionAccessesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_PartitionAccesses_PartitionGuid",
                table: "PartitionAccesses",
                column: "PartitionGuid");

            migrationBuilder.CreateIndex(
                name: "IX_PartitionAccesses_UserGuid",
                table: "PartitionAccesses",
                column: "UserGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Partitions_ArticleGuid",
                table: "Partitions",
                column: "ArticleGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Partitions_ItemGuid",
                table: "Partitions",
                column: "ItemGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Partitions_ParentPartitionGuid",
                table: "Partitions",
                column: "ParentPartitionGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Partitions_UserGroupGuid",
                table: "Partitions",
                column: "UserGroupGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Partitions_UserGuid",
                table: "Partitions",
                column: "UserGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartitionAccesses")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PartitionAccessesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropTable(
                name: "Partitions")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PartitionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");
        }
    }
}