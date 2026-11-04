using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IntroduceEventBaseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_item_events",
                table: "item_events");

            migrationBuilder.DropIndex(
                name: "ix_item_events_article_guid_occured_at",
                table: "item_events");

            migrationBuilder.DropIndex(
                name: "ix_item_events_occured_at",
                table: "item_events");

            migrationBuilder.DropColumn(
                name: "occured_at",
                table: "item_events");

            migrationBuilder.RenameColumn(
                name: "event_type",
                table: "item_events",
                newName: "item_event_type");

            migrationBuilder.AddPrimaryKey(
                name: "pk_events",
                table: "item_events",
                column: "guid");

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<int>(type: "integer", nullable: false),
                    occured_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.guid);
                });

            migrationBuilder.CreateIndex(
                name: "ix_events_event_type",
                table: "events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "ix_events_occured_at",
                table: "events",
                column: "occured_at");

            migrationBuilder.AddForeignKey(
                name: "fk_item_events_events_guid",
                table: "item_events",
                column: "guid",
                principalTable: "events",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_item_events_events_guid",
                table: "item_events");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropPrimaryKey(
                name: "pk_events",
                table: "item_events");

            migrationBuilder.RenameColumn(
                name: "item_event_type",
                table: "item_events",
                newName: "event_type");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "occured_at",
                table: "item_events",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddPrimaryKey(
                name: "pk_item_events",
                table: "item_events",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "ix_item_events_article_guid_occured_at",
                table: "item_events",
                columns: new[] { "article_guid", "occured_at" });

            migrationBuilder.CreateIndex(
                name: "ix_item_events_occured_at",
                table: "item_events",
                column: "occured_at");
        }
    }
}
