using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMoviment2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "item_parent_containers_history");

            migrationBuilder.DropColumn(
                name: "last_parent_item_container_changed_at",
                table: "items");

            migrationBuilder.CreateTable(
                name: "item_moviments",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    item_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    moved_to_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_moviments", x => x.guid);
                    table.ForeignKey(
                        name: "fk_item_moviments_items_item_guid",
                        column: x => x.item_guid,
                        principalTable: "items",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_item_moviments_items_moved_to_container_guid",
                        column: x => x.moved_to_container_guid,
                        principalTable: "items",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_item_moviments_item_guid",
                table: "item_moviments",
                column: "item_guid");

            migrationBuilder.CreateIndex(
                name: "ix_item_moviments_moved_to_container_guid",
                table: "item_moviments",
                column: "moved_to_container_guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "item_moviments");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_parent_item_container_changed_at",
                table: "items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateTable(
                name: "item_parent_containers_history",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    item_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_item_contianer_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    valid_at = table.Column<NpgsqlRange<DateTimeOffset>>(type: "tstzrange", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_parent_containers_history", x => x.guid);
                    table.ForeignKey(
                        name: "fk_item_parent_containers_history_items_item_guid",
                        column: x => x.item_guid,
                        principalTable: "items",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_item_parent_containers_history_items_parent_item_contianer_",
                        column: x => x.parent_item_contianer_guid,
                        principalTable: "items",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_item_parent_containers_history_item_guid",
                table: "item_parent_containers_history",
                column: "item_guid");

            migrationBuilder.CreateIndex(
                name: "ix_item_parent_containers_history_parent_item_contianer_guid",
                table: "item_parent_containers_history",
                column: "parent_item_contianer_guid");
        }
    }
}
