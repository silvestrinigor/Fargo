using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ChangesIDK : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_UserPermission",
            table: "UserPermission");

        migrationBuilder.AddColumn<Guid>(
            name: "Guid",
            table: "UserPermission",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddUniqueConstraint(
            name: "AK_UserPermission_UserGuid_Action",
            table: "UserPermission",
            columns: new[] { "UserGuid", "Action" });

        migrationBuilder.AddPrimaryKey(
            name: "PK_UserPermission",
            table: "UserPermission",
            column: "Guid");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropUniqueConstraint(
            name: "AK_UserPermission_UserGuid_Action",
            table: "UserPermission");

        migrationBuilder.DropPrimaryKey(
            name: "PK_UserPermission",
            table: "UserPermission");

        migrationBuilder.DropColumn(
            name: "Guid",
            table: "UserPermission");

        migrationBuilder.AddPrimaryKey(
            name: "PK_UserPermission",
            table: "UserPermission",
            columns: new[] { "UserGuid", "Action" });
    }
}
