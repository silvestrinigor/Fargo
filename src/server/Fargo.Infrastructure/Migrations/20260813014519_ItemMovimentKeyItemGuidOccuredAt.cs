using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ItemMovimentKeyItemGuidOccuredAt : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "pk_item_moviments",
            table: "item_moviments");

        migrationBuilder.DropIndex(
            name: "ix_item_moviments_item_guid",
            table: "item_moviments");

        migrationBuilder.DropColumn(
            name: "guid",
            table: "item_moviments");

        migrationBuilder.AddPrimaryKey(
            name: "pk_item_moviments",
            table: "item_moviments",
            columns: new[] { "item_guid", "occurred_at" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "pk_item_moviments",
            table: "item_moviments");

        migrationBuilder.AddColumn<Guid>(
            name: "guid",
            table: "item_moviments",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddPrimaryKey(
            name: "pk_item_moviments",
            table: "item_moviments",
            column: "guid");

        migrationBuilder.CreateIndex(
            name: "ix_item_moviments_item_guid",
            table: "item_moviments",
            column: "item_guid");
    }
}
