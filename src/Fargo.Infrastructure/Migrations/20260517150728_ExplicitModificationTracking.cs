using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ExplicitModificationTracking : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsEditStarted",
            table: "Users",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "ModificationTypes",
            table: "Users",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<bool>(
            name: "IsEditStarted",
            table: "UserGroups",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "ModificationTypes",
            table: "UserGroups",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<bool>(
            name: "IsEditStarted",
            table: "Partitions",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "ModificationTypes",
            table: "Partitions",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<bool>(
            name: "IsEditStarted",
            table: "Items",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "ModificationTypes",
            table: "Items",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsEditStarted",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "ModificationTypes",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "IsEditStarted",
            table: "UserGroups");

        migrationBuilder.DropColumn(
            name: "ModificationTypes",
            table: "UserGroups");

        migrationBuilder.DropColumn(
            name: "IsEditStarted",
            table: "Partitions");

        migrationBuilder.DropColumn(
            name: "ModificationTypes",
            table: "Partitions");

        migrationBuilder.DropColumn(
            name: "IsEditStarted",
            table: "Items");

        migrationBuilder.DropColumn(
            name: "ModificationTypes",
            table: "Items");
    }
}
