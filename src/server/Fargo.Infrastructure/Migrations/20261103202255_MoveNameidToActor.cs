using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveNameidToActor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_nameid",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_service_accounts_client_id",
                table: "service_accounts");

            migrationBuilder.DropColumn(
                name: "nameid",
                table: "users");

            migrationBuilder.DropColumn(
                name: "client_id",
                table: "service_accounts");

            migrationBuilder.AddColumn<string>(
                name: "nameid",
                table: "actors",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_actors_nameid",
                table: "actors",
                column: "nameid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_actors_nameid",
                table: "actors");

            migrationBuilder.DropColumn(
                name: "nameid",
                table: "actors");

            migrationBuilder.AddColumn<string>(
                name: "nameid",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "client_id",
                table: "service_accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_users_nameid",
                table: "users",
                column: "nameid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_service_accounts_client_id",
                table: "service_accounts",
                column: "client_id",
                unique: true);
        }
    }
}
