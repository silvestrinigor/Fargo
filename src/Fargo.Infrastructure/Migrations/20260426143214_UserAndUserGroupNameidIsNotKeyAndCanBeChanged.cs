using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserAndUserGroupNameidIsNotKeyAndCanBeChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_Nameid",
                table: "Users");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_UserGroups_Nameid",
                table: "UserGroups");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Nameid",
                table: "Users",
                column: "Nameid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_Nameid",
                table: "UserGroups",
                column: "Nameid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Nameid",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserGroups_Nameid",
                table: "UserGroups");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_Nameid",
                table: "Users",
                column: "Nameid");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_UserGroups_Nameid",
                table: "UserGroups",
                column: "Nameid");
        }
    }
}
