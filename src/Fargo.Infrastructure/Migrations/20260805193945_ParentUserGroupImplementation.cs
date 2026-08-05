using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ParentUserGroupImplementation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "parent_user_group_guid",
                table: "user_groups",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_groups_parent_user_group_guid",
                table: "user_groups",
                column: "parent_user_group_guid");

            migrationBuilder.AddForeignKey(
                name: "fk_user_groups_user_groups_parent_user_group_guid",
                table: "user_groups",
                column: "parent_user_group_guid",
                principalTable: "user_groups",
                principalColumn: "guid",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_groups_user_groups_parent_user_group_guid",
                table: "user_groups");

            migrationBuilder.DropIndex(
                name: "ix_user_groups_parent_user_group_guid",
                table: "user_groups");

            migrationBuilder.DropColumn(
                name: "parent_user_group_guid",
                table: "user_groups");
        }
    }
}
