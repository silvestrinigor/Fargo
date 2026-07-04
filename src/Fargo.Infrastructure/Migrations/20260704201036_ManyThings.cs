using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ManyThings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntityPartitionEvents");

            migrationBuilder.DropTable(
                name: "ItemMovements");

            migrationBuilder.DropTable(
                name: "EntityEvents");

            migrationBuilder.DropColumn(
                name: "EditedByGuid",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsEditStarted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ModificationTypes",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EditedByGuid",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "IsEditStarted",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "ModificationTypes",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "EditedByGuid",
                table: "Partitions");

            migrationBuilder.DropColumn(
                name: "IsEditStarted",
                table: "Partitions");

            migrationBuilder.DropColumn(
                name: "ModificationTypes",
                table: "Partitions");

            migrationBuilder.DropColumn(
                name: "EditedByGuid",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "IsEditStarted",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ModificationTypes",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "EditedByGuid",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ModificationTypes",
                table: "Articles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EditedByGuid",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEditStarted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ModificationTypes",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "EditedByGuid",
                table: "UserGroups",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEditStarted",
                table: "UserGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ModificationTypes",
                table: "UserGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "EditedByGuid",
                table: "Partitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEditStarted",
                table: "Partitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ModificationTypes",
                table: "Partitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "EditedByGuid",
                table: "Items",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEditStarted",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ModificationTypes",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "EditedByGuid",
                table: "Articles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModificationTypes",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EntityEvents",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityEvents", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "EntityPartitionEvents",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartitionGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityPartitionEvents", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_EntityPartitionEvents_EntityEvents_Guid",
                        column: x => x.Guid,
                        principalTable: "EntityEvents",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemMovements",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromParentContainerGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToParentContainerGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemMovements", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_ItemMovements_EntityEvents_Guid",
                        column: x => x.Guid,
                        principalTable: "EntityEvents",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_EntityPartitionEvents_PartitionGuid",
                table: "EntityPartitionEvents",
                column: "PartitionGuid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMovements_FromParentContainerGuid",
                table: "ItemMovements",
                column: "FromParentContainerGuid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMovements_ToParentContainerGuid",
                table: "ItemMovements",
                column: "ToParentContainerGuid");
        }
    }
}
