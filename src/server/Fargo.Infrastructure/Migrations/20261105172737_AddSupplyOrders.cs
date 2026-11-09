using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddSupplyOrders : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "supply_orders",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                status = table.Column<byte>(type: "smallint", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                placed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                receiving_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_entities", x => x.guid);
                table.ForeignKey(
                    name: "fk_supply_orders_entities_guid",
                    column: x => x.guid,
                    principalTable: "entities",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_supply_orders_items_receiving_container_guid",
                    column: x => x.receiving_container_guid,
                    principalTable: "items",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "supply_order_lines",
            columns: table => new
            {
                supply_order_guid = table.Column<Guid>(type: "uuid", nullable: false),
                article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                ordered_quantity = table.Column<double>(type: "double precision", nullable: false),
                received_quantity = table.Column<double>(type: "double precision", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_supply_order_lines", x => new { x.supply_order_guid, x.article_guid });
                table.ForeignKey(
                    name: "fk_supply_order_lines_articles_article_guid",
                    column: x => x.article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_supply_order_lines_supply_orders_supply_order_guid",
                    column: x => x.supply_order_guid,
                    principalTable: "supply_orders",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "supply_order_partitions",
            columns: table => new
            {
                supply_order_guid = table.Column<Guid>(type: "uuid", nullable: false),
                partition_guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_supply_order_partitions", x => new { x.supply_order_guid, x.partition_guid });
                table.ForeignKey(
                    name: "fk_supply_order_partitions_partitions_partition_guid",
                    column: x => x.partition_guid,
                    principalTable: "partitions",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_supply_order_partitions_supply_orders_supply_order_guid",
                    column: x => x.supply_order_guid,
                    principalTable: "supply_orders",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_supply_order_lines_article_guid",
            table: "supply_order_lines",
            column: "article_guid");

        migrationBuilder.CreateIndex(
            name: "ix_supply_order_partitions_partition_guid",
            table: "supply_order_partitions",
            column: "partition_guid");

        migrationBuilder.CreateIndex(
            name: "ix_supply_orders_receiving_container_guid",
            table: "supply_orders",
            column: "receiving_container_guid");

        migrationBuilder.CreateIndex(
            name: "ix_supply_orders_status",
            table: "supply_orders",
            column: "status");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "supply_order_lines");

        migrationBuilder.DropTable(
            name: "supply_order_partitions");

        migrationBuilder.DropTable(
            name: "supply_orders");
    }
}
