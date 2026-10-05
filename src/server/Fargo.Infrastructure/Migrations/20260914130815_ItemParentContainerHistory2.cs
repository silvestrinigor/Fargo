using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ItemParentContainerHistory2 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "item_parent_container_history",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                valid_period = table.Column<NpgsqlRange<DateTime>>(type: "tstzrange", nullable: false, defaultValueSql: "tstzrange(now(), 'infinity', '[)')"),
                item_guid = table.Column<Guid>(type: "uuid", nullable: false),
                parent_item_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                removed_from_containers = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_item_parent_container_history", x => x.id);
                table.ForeignKey(
                    name: "fk_item_parent_container_history_items_item_guid",
                    column: x => x.item_guid,
                    principalTable: "items",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_item_parent_container_history_items_parent_item_container_g",
                    column: x => x.parent_item_container_guid,
                    principalTable: "items",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "ix_item_parent_container_history_item_guid",
            table: "item_parent_container_history",
            column: "item_guid");

        migrationBuilder.CreateIndex(
            name: "ix_item_parent_container_history_parent_item_container_guid",
            table: "item_parent_container_history",
            column: "parent_item_container_guid");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "item_parent_container_history");
    }
}
