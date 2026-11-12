using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddAuthorizationStamp : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "authorization_stamps",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false),
                value = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_authorization_stamps", x => x.id);
            });

        migrationBuilder.InsertData(
            table: "authorization_stamps",
            columns: new[] { "id", "value" },
            values: new object[] { 1, new Guid("6b2f3c1e-9a4d-4e7b-8c5f-1d2e3f4a5b6c") });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "authorization_stamps");
    }
}
