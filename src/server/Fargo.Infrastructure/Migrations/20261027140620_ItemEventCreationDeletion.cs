using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemEventCreationDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "deletes_item_event_guid",
                table: "item_events",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "item_container_guid",
                table: "item_events",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_item_events_deletes_item_event_guid",
                table: "item_events",
                column: "deletes_item_event_guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_item_events_deletes_item_event_guid",
                table: "item_events");

            migrationBuilder.DropColumn(
                name: "deletes_item_event_guid",
                table: "item_events");

            migrationBuilder.DropColumn(
                name: "item_container_guid",
                table: "item_events");
        }
    }
}
