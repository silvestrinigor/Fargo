using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AuthTokenRevocation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "AuthVersion",
            table: "Users",
            type: "uniqueidentifier",
            nullable: false,
            defaultValueSql: "newid()");

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "RevokedAt",
            table: "RefreshTokens",
            type: "datetimeoffset",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AuthVersion",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "RevokedAt",
            table: "RefreshTokens");
    }
}
