using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class EntityEventLedger : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "EntityEvents",
            columns: table => new
            {
                Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EntityType = table.Column<int>(type: "int", nullable: false),
                EventType = table.Column<int>(type: "int", nullable: false),
                EntityGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ActorGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EntityEvents", x => x.Guid);
            });

        migrationBuilder.CreateIndex(
            name: "IX_EntityEvents_ActorGuid",
            table: "EntityEvents",
            column: "ActorGuid");

        migrationBuilder.CreateIndex(
            name: "IX_EntityEvents_EntityGuid_OccurredAt",
            table: "EntityEvents",
            columns: new[] { "EntityGuid", "OccurredAt" });

        migrationBuilder.CreateIndex(
            name: "IX_EntityEvents_EntityType",
            table: "EntityEvents",
            column: "EntityType");

        migrationBuilder.CreateIndex(
            name: "IX_EntityEvents_EventType",
            table: "EntityEvents",
            column: "EventType");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "EntityEvents");
    }
}
