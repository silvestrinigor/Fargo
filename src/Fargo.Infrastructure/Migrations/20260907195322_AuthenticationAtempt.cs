using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AuthenticationAtempt : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "authentication_attempt",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                actor_identifier = table.Column<string>(type: "text", nullable: false),
                ip_address = table.Column<string>(type: "text", nullable: false),
                occured_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                successful = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_authentication_attempt", x => x.guid);
            });

        migrationBuilder.CreateIndex(
            name: "ix_authentication_attempt_ip_address_actor_identifier_occured_",
            table: "authentication_attempt",
            columns: new[] { "ip_address", "actor_identifier", "occured_at", "successful" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "authentication_attempt");
    }
}
