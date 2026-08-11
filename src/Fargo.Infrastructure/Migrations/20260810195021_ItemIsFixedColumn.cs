using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ItemIsFixedColumn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "is_fixed",
            table: "items",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "is_fixed",
            table: "items");
    }
}
