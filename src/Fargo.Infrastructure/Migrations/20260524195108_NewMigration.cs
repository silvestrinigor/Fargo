using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkspaceCommands");

            migrationBuilder.DropTable(
                name: "Workspaces");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Workspaces",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RolledBackAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceCommands",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkspaceGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommandId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CommandType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CommandVersion = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExecutedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReservedEntityGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReservedEntityKind = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceCommands", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_WorkspaceCommands_Workspaces_WorkspaceGuid",
                        column: x => x.WorkspaceGuid,
                        principalTable: "Workspaces",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceCommands_CommandType",
                table: "WorkspaceCommands",
                column: "CommandType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceCommands_ReservedEntityGuid",
                table: "WorkspaceCommands",
                column: "ReservedEntityGuid",
                unique: true,
                filter: "[ReservedEntityGuid] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceCommands_WorkspaceGuid_CommandId",
                table: "WorkspaceCommands",
                columns: new[] { "WorkspaceGuid", "CommandId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceCommands_WorkspaceGuid_Sequence",
                table: "WorkspaceCommands",
                columns: new[] { "WorkspaceGuid", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_ActorGuid",
                table: "Workspaces",
                column: "ActorGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_Status",
                table: "Workspaces",
                column: "Status");
        }
    }
}
