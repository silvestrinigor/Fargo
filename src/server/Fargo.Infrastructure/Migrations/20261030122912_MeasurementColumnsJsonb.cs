using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class MeasurementColumnsJsonb : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Mass, max mass and the X/Y/Z dimensions move from a formatted text
        // string (e.g. "2.5 kg") to a JSON object ({ "value": 2.5, "unit":
        // "kg" }) stored in jsonb, so the value and unit are individually
        // queryable. Old text values are not valid JSON and cannot be cast,
        // and no data is preserved, so the columns are dropped and re-added.
        migrationBuilder.DropColumn(name: "mass", table: "articles");
        migrationBuilder.DropColumn(name: "max_mass", table: "article_containers");
        migrationBuilder.DropColumn(name: "x", table: "article_dimensions");
        migrationBuilder.DropColumn(name: "y", table: "article_dimensions");
        migrationBuilder.DropColumn(name: "z", table: "article_dimensions");

        migrationBuilder.AddColumn<string>(
            name: "mass", table: "articles", type: "jsonb", nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "max_mass", table: "article_containers", type: "jsonb", nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "x", table: "article_dimensions", type: "jsonb", nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "y", table: "article_dimensions", type: "jsonb", nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "z", table: "article_dimensions", type: "jsonb", nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "mass", table: "articles");
        migrationBuilder.DropColumn(name: "max_mass", table: "article_containers");
        migrationBuilder.DropColumn(name: "x", table: "article_dimensions");
        migrationBuilder.DropColumn(name: "y", table: "article_dimensions");
        migrationBuilder.DropColumn(name: "z", table: "article_dimensions");

        migrationBuilder.AddColumn<string>(
            name: "mass", table: "articles", type: "text", nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "max_mass", table: "article_containers", type: "text", nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "x", table: "article_dimensions", type: "text", nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "y", table: "article_dimensions", type: "text", nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "z", table: "article_dimensions", type: "text", nullable: true);
    }
}
