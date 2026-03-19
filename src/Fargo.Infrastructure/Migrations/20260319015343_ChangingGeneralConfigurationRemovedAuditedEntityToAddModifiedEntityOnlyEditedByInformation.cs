using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangingGeneralConfigurationRemovedAuditedEntityToAddModifiedEntityOnlyEditedByInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Partitions_Articles_ArticleGuid",
                table: "Partitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Partitions_Items_ItemGuid",
                table: "Partitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Partitions_UserGroups_UserGroupGuid",
                table: "Partitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Partitions_Users_UserGuid",
                table: "Partitions");

            migrationBuilder.DropTable(
                name: "PartitionAccesses")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PartitionAccessesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropIndex(
                name: "IX_Partitions_ArticleGuid",
                table: "Partitions");

            migrationBuilder.DropIndex(
                name: "IX_Partitions_ItemGuid",
                table: "Partitions");

            migrationBuilder.DropIndex(
                name: "IX_Partitions_UserGroupGuid",
                table: "Partitions");

            migrationBuilder.DropIndex(
                name: "IX_Partitions_UserGuid",
                table: "Partitions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedByGuid",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "CreatedByGuid",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "ArticleGuid",
                table: "Partitions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Partitions");

            migrationBuilder.DropColumn(
                name: "CreatedByGuid",
                table: "Partitions");

            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "Partitions");

            migrationBuilder.DropColumn(
                name: "ItemGuid",
                table: "Partitions");

            migrationBuilder.DropColumn(
                name: "UserGroupGuid",
                table: "Partitions");

            migrationBuilder.DropColumn(
                name: "UserGuid",
                table: "Partitions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CreatedByGuid",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "CreatedByGuid",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "Articles");

            migrationBuilder.CreateTable(
                name: "ArticlePartition",
                columns: table => new
                {
                    ArticleMembersGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartitionsGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticlePartition", x => new { x.ArticleMembersGuid, x.PartitionsGuid });
                    table.ForeignKey(
                        name: "FK_ArticlePartition_Articles_ArticleMembersGuid",
                        column: x => x.ArticleMembersGuid,
                        principalTable: "Articles",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticlePartition_Partitions_PartitionsGuid",
                        column: x => x.PartitionsGuid,
                        principalTable: "Partitions",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ArticlePartitionHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateTable(
                name: "ItemPartition",
                columns: table => new
                {
                    ItemMembersGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartitionsGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemPartition", x => new { x.ItemMembersGuid, x.PartitionsGuid });
                    table.ForeignKey(
                        name: "FK_ItemPartition_Items_ItemMembersGuid",
                        column: x => x.ItemMembersGuid,
                        principalTable: "Items",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemPartition_Partitions_PartitionsGuid",
                        column: x => x.PartitionsGuid,
                        principalTable: "Partitions",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ItemPartitionHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateTable(
                name: "PartitionUser",
                columns: table => new
                {
                    PartitionsGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserMembersGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartitionUser", x => new { x.PartitionsGuid, x.UserMembersGuid });
                    table.ForeignKey(
                        name: "FK_PartitionUser_Partitions_PartitionsGuid",
                        column: x => x.PartitionsGuid,
                        principalTable: "Partitions",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartitionUser_Users_UserMembersGuid",
                        column: x => x.UserMembersGuid,
                        principalTable: "Users",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PartitionUserHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateTable(
                name: "PartitionUserGroup",
                columns: table => new
                {
                    PartitionsGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGroupMembersGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartitionUserGroup", x => new { x.PartitionsGuid, x.UserGroupMembersGuid });
                    table.ForeignKey(
                        name: "FK_PartitionUserGroup_Partitions_PartitionsGuid",
                        column: x => x.PartitionsGuid,
                        principalTable: "Partitions",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartitionUserGroup_UserGroups_UserGroupMembersGuid",
                        column: x => x.UserGroupMembersGuid,
                        principalTable: "UserGroups",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PartitionUserGroupHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateTable(
                name: "UserGroupPartitionAccesses",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGroupGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartitionGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupPartitionAccesses", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_UserGroupPartitionAccesses_Partitions_PartitionGuid",
                        column: x => x.PartitionGuid,
                        principalTable: "Partitions",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroupPartitionAccesses_UserGroups_UserGroupGuid",
                        column: x => x.UserGroupGuid,
                        principalTable: "UserGroups",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "UserGroupPartitionAccessesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateTable(
                name: "UserPartitionAccesses",
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
                    table.PrimaryKey("PK_UserPartitionAccesses", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_UserPartitionAccesses_Partitions_PartitionGuid",
                        column: x => x.PartitionGuid,
                        principalTable: "Partitions",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPartitionAccesses_Users_UserGuid",
                        column: x => x.UserGuid,
                        principalTable: "Users",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "UserPartitionAccessesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_ArticlePartition_PartitionsGuid",
                table: "ArticlePartition",
                column: "PartitionsGuid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPartition_PartitionsGuid",
                table: "ItemPartition",
                column: "PartitionsGuid");

            migrationBuilder.CreateIndex(
                name: "IX_PartitionUser_UserMembersGuid",
                table: "PartitionUser",
                column: "UserMembersGuid");

            migrationBuilder.CreateIndex(
                name: "IX_PartitionUserGroup_UserGroupMembersGuid",
                table: "PartitionUserGroup",
                column: "UserGroupMembersGuid");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupPartitionAccesses_PartitionGuid",
                table: "UserGroupPartitionAccesses",
                column: "PartitionGuid");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupPartitionAccesses_UserGroupGuid",
                table: "UserGroupPartitionAccesses",
                column: "UserGroupGuid");

            migrationBuilder.CreateIndex(
                name: "IX_UserPartitionAccesses_PartitionGuid",
                table: "UserPartitionAccesses",
                column: "PartitionGuid");

            migrationBuilder.CreateIndex(
                name: "IX_UserPartitionAccesses_UserGuid",
                table: "UserPartitionAccesses",
                column: "UserGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticlePartition")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ArticlePartitionHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropTable(
                name: "ItemPartition")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ItemPartitionHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropTable(
                name: "PartitionUser")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PartitionUserHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropTable(
                name: "PartitionUserGroup")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PartitionUserGroupHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropTable(
                name: "UserGroupPartitionAccesses")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "UserGroupPartitionAccessesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropTable(
                name: "UserPartitionAccesses")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "UserPartitionAccessesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Users",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByGuid",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EditedAt",
                table: "Users",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "UserGroups",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByGuid",
                table: "UserGroups",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EditedAt",
                table: "UserGroups",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ArticleGuid",
                table: "Partitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Partitions",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByGuid",
                table: "Partitions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EditedAt",
                table: "Partitions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ItemGuid",
                table: "Partitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserGroupGuid",
                table: "Partitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserGuid",
                table: "Partitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Items",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByGuid",
                table: "Items",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EditedAt",
                table: "Items",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Articles",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByGuid",
                table: "Articles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EditedAt",
                table: "Articles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PartitionAccesses",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartitionGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                name: "IX_Partitions_ArticleGuid",
                table: "Partitions",
                column: "ArticleGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Partitions_ItemGuid",
                table: "Partitions",
                column: "ItemGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Partitions_UserGroupGuid",
                table: "Partitions",
                column: "UserGroupGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Partitions_UserGuid",
                table: "Partitions",
                column: "UserGuid");

            migrationBuilder.CreateIndex(
                name: "IX_PartitionAccesses_PartitionGuid",
                table: "PartitionAccesses",
                column: "PartitionGuid");

            migrationBuilder.CreateIndex(
                name: "IX_PartitionAccesses_UserGuid",
                table: "PartitionAccesses",
                column: "UserGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Partitions_Articles_ArticleGuid",
                table: "Partitions",
                column: "ArticleGuid",
                principalTable: "Articles",
                principalColumn: "Guid");

            migrationBuilder.AddForeignKey(
                name: "FK_Partitions_Items_ItemGuid",
                table: "Partitions",
                column: "ItemGuid",
                principalTable: "Items",
                principalColumn: "Guid");

            migrationBuilder.AddForeignKey(
                name: "FK_Partitions_UserGroups_UserGroupGuid",
                table: "Partitions",
                column: "UserGroupGuid",
                principalTable: "UserGroups",
                principalColumn: "Guid");

            migrationBuilder.AddForeignKey(
                name: "FK_Partitions_Users_UserGuid",
                table: "Partitions",
                column: "UserGuid",
                principalTable: "Users",
                principalColumn: "Guid");
        }
    }
}
