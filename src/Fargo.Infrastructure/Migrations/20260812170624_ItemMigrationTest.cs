using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ItemMigrationTest : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "pk_item_parent_containers_history",
            table: "item_parent_containers_history");

        migrationBuilder.DropIndex(
            name: "ix_item_parent_containers_history_item_guid",
            table: "item_parent_containers_history");

        migrationBuilder.AlterColumn<Guid>(
            name: "parent_item_contianer_guid",
            table: "item_parent_containers_history",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);

        migrationBuilder.AddPrimaryKey(
            name: "pk_item_parent_containers_history",
            table: "item_parent_containers_history",
            columns: new[] { "item_guid", "parent_item_contianer_guid" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "pk_item_parent_containers_history",
            table: "item_parent_containers_history");

        migrationBuilder.AlterColumn<Guid>(
            name: "parent_item_contianer_guid",
            table: "item_parent_containers_history",
            type: "uuid",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uuid");

        migrationBuilder.AddPrimaryKey(
            name: "pk_item_parent_containers_history",
            table: "item_parent_containers_history",
            column: "guid");

        migrationBuilder.CreateIndex(
            name: "ix_item_parent_containers_history_item_guid",
            table: "item_parent_containers_history",
            column: "item_guid");
    }
}
