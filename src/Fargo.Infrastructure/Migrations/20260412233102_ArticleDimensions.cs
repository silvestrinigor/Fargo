using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ArticleDimensions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<double>(
            name: "LengthX",
            table: "Articles",
            type: "float",
            nullable: true);

        migrationBuilder.AddColumn<double>(
            name: "LengthY",
            table: "Articles",
            type: "float",
            nullable: true);

        migrationBuilder.AddColumn<double>(
            name: "LengthZ",
            table: "Articles",
            type: "float",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LengthX",
            table: "Articles");

        migrationBuilder.DropColumn(
            name: "LengthY",
            table: "Articles");

        migrationBuilder.DropColumn(
            name: "LengthZ",
            table: "Articles");
    }
}
