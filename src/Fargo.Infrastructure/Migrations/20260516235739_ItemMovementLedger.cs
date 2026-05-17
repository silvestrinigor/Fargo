using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMovementLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemMovements",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromParentContainerGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToParentContainerGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActorGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemMovements", x => x.Guid);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemMovements_ActorGuid",
                table: "ItemMovements",
                column: "ActorGuid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMovements_ItemGuid_OccurredAt",
                table: "ItemMovements",
                columns: new[] { "ItemGuid", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemMovements");
        }
    }
}
