using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class Changes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserGroupUsers");

        migrationBuilder.CreateTable(
            name: "UserUserGroup",
            columns: table => new
            {
                UserGroupsGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UsersGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserUserGroup", x => new { x.UserGroupsGuid, x.UsersGuid });
                table.ForeignKey(
                    name: "FK_UserUserGroup_UserGroups_UserGroupsGuid",
                    column: x => x.UserGroupsGuid,
                    principalTable: "UserGroups",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserUserGroup_Users_UsersGuid",
                    column: x => x.UsersGuid,
                    principalTable: "Users",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "UserUserGroupHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.CreateIndex(
            name: "IX_UserUserGroup_UsersGuid",
            table: "UserUserGroup",
            column: "UsersGuid");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserUserGroup")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "UserUserGroupHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.CreateTable(
            name: "UserGroupUsers",
            columns: table => new
            {
                UserGroupsGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserGroupUsers", x => new { x.UserGroupsGuid, x.UserGuid });
                table.ForeignKey(
                    name: "FK_UserGroupUsers_UserGroups_UserGroupsGuid",
                    column: x => x.UserGroupsGuid,
                    principalTable: "UserGroups",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserGroupUsers_Users_UserGuid",
                    column: x => x.UserGuid,
                    principalTable: "Users",
                    principalColumn: "Guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserGroupUsers_UserGuid",
            table: "UserGroupUsers",
            column: "UserGuid");
    }
}
