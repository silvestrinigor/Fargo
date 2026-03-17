using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class PartitionN : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsEditable",
            table: "Partitions");

        migrationBuilder.DropColumn(
            name: "IsGlobal",
            table: "Partitions");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsEditable",
            table: "Partitions",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsGlobal",
            table: "Partitions",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }
}
