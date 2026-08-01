using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserGroupPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_group_permissions");

            migrationBuilder.AddColumn<string>(
                name: "permissions",
                table: "user_groups",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "permissions",
                table: "user_groups");

            migrationBuilder.CreateTable(
                name: "user_group_permissions",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_group_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_group_permissions", x => x.guid);
                    table.UniqueConstraint("ak_user_group_permissions_user_group_guid_action", x => new { x.user_group_guid, x.action });
                    table.ForeignKey(
                        name: "fk_user_group_permissions_user_groups_user_group_guid",
                        column: x => x.user_group_guid,
                        principalTable: "user_groups",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
