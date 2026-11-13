using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddIdempotencyKeys : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "idempotency_keys",
            columns: table => new
            {
                actor_guid = table.Column<Guid>(type: "uuid", nullable: false),
                key = table.Column<Guid>(type: "uuid", nullable: false),
                command_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                response = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_idempotency_keys", x => new { x.actor_guid, x.key });
                table.ForeignKey(
                    name: "fk_idempotency_keys_actors_actor_guid",
                    column: x => x.actor_guid,
                    principalTable: "actors",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_idempotency_keys_expires_at",
            table: "idempotency_keys",
            column: "expires_at");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "idempotency_keys");
    }
}
