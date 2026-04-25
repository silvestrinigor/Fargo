using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    EntityGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<int>(type: "int", nullable: false),
                    ApiClientGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActorGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Guid);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_ActorGuid",
                table: "Events",
                column: "ActorGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ApiClientGuid",
                table: "Events",
                column: "ApiClientGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EntityGuid",
                table: "Events",
                column: "EntityGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EntityGuid_OccurredAt",
                table: "Events",
                columns: new[] { "EntityGuid", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_EntityType",
                table: "Events",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_Events_OccurredAt",
                table: "Events",
                column: "OccurredAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
