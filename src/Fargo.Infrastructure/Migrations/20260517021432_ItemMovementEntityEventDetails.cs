using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ItemMovementEntityEventDetails : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ItemMovements_ActorGuid",
            table: "ItemMovements");

        migrationBuilder.DropIndex(
            name: "IX_ItemMovements_ItemGuid_OccurredAt",
            table: "ItemMovements");

        migrationBuilder.Sql("""
            INSERT INTO [EntityEvents] ([Guid], [EntityType], [EventType], [EntityGuid], [ActorGuid], [OccurredAt])
            SELECT [Guid], 1, 4, [ItemGuid], [ActorGuid], [OccurredAt]
            FROM [ItemMovements] AS [movement]
            WHERE NOT EXISTS
            (
                SELECT 1
                FROM [EntityEvents] AS [event]
                WHERE [event].[Guid] = [movement].[Guid]
            )
            """);

        migrationBuilder.DropColumn(
            name: "ActorGuid",
            table: "ItemMovements");

        migrationBuilder.DropColumn(
            name: "ItemGuid",
            table: "ItemMovements");

        migrationBuilder.DropColumn(
            name: "OccurredAt",
            table: "ItemMovements");

        migrationBuilder.CreateIndex(
            name: "IX_ItemMovements_FromParentContainerGuid",
            table: "ItemMovements",
            column: "FromParentContainerGuid");

        migrationBuilder.CreateIndex(
            name: "IX_ItemMovements_ToParentContainerGuid",
            table: "ItemMovements",
            column: "ToParentContainerGuid");

        migrationBuilder.AddForeignKey(
            name: "FK_ItemMovements_EntityEvents_Guid",
            table: "ItemMovements",
            column: "Guid",
            principalTable: "EntityEvents",
            principalColumn: "Guid",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ItemMovements_EntityEvents_Guid",
            table: "ItemMovements");

        migrationBuilder.DropIndex(
            name: "IX_ItemMovements_FromParentContainerGuid",
            table: "ItemMovements");

        migrationBuilder.DropIndex(
            name: "IX_ItemMovements_ToParentContainerGuid",
            table: "ItemMovements");

        migrationBuilder.AddColumn<Guid>(
            name: "ActorGuid",
            table: "ItemMovements",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<Guid>(
            name: "ItemGuid",
            table: "ItemMovements",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "OccurredAt",
            table: "ItemMovements",
            type: "datetimeoffset",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.Sql("""
            UPDATE [movement]
            SET
                [movement].[ItemGuid] = [event].[EntityGuid],
                [movement].[ActorGuid] = [event].[ActorGuid],
                [movement].[OccurredAt] = [event].[OccurredAt]
            FROM [ItemMovements] AS [movement]
            INNER JOIN [EntityEvents] AS [event]
                ON [event].[Guid] = [movement].[Guid]
            WHERE [event].[EntityType] = 1
                AND [event].[EventType] = 4
            """);

        migrationBuilder.Sql("""
            DELETE [event]
            FROM [EntityEvents] AS [event]
            INNER JOIN [ItemMovements] AS [movement]
                ON [movement].[Guid] = [event].[Guid]
            WHERE [event].[EntityType] = 1
                AND [event].[EventType] = 4
            """);

        migrationBuilder.CreateIndex(
            name: "IX_ItemMovements_ActorGuid",
            table: "ItemMovements",
            column: "ActorGuid");

        migrationBuilder.CreateIndex(
            name: "IX_ItemMovements_ItemGuid_OccurredAt",
            table: "ItemMovements",
            columns: new[] { "ItemGuid", "OccurredAt" });
    }
}
