using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class UserFirstLastNamePasswordExpiration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<TimeSpan>(
            name: "DefaultPasswordExpirationTimeSpan",
            table: "Users",
            type: "time",
            nullable: false,
            defaultValue: new TimeSpan(0, 0, 0, 0, 0));

        migrationBuilder.AddColumn<string>(
            name: "FirstName",
            table: "Users",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "LastName",
            table: "Users",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "RequirePasswordChangeAt",
            table: "Users",
            type: "datetimeoffset",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DefaultPasswordExpirationTimeSpan",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "FirstName",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "LastName",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "RequirePasswordChangeAt",
            table: "Users");
    }
}
