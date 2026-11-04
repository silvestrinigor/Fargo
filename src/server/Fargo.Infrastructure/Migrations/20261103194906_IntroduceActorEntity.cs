using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class IntroduceActorEntity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "service_account_partitions");

        migrationBuilder.DropTable(
            name: "user_partition_accesses");

        migrationBuilder.DropTable(
            name: "user_partitions");

        migrationBuilder.DropColumn(
            name: "description",
            table: "users");

        migrationBuilder.DropColumn(
            name: "is_active",
            table: "users");

        migrationBuilder.DropColumn(
            name: "permissions",
            table: "users");

        migrationBuilder.DropColumn(
            name: "xmin",
            table: "users");

        migrationBuilder.DropColumn(
            name: "description",
            table: "service_accounts");

        migrationBuilder.DropColumn(
            name: "is_active",
            table: "service_accounts");

        migrationBuilder.DropColumn(
            name: "partition_access_guids",
            table: "service_accounts");

        migrationBuilder.DropColumn(
            name: "permissions",
            table: "service_accounts");

        migrationBuilder.DropColumn(
            name: "xmin",
            table: "service_accounts");

        migrationBuilder.RenameColumn(
            name: "guid",
            table: "users",
            newName: "actor_guid");

        migrationBuilder.RenameColumn(
            name: "guid",
            table: "service_accounts",
            newName: "actor_guid");

        migrationBuilder.CreateTable(
            name: "actors",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                actor_type = table.Column<byte>(type: "smallint", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                permissions = table.Column<string>(type: "jsonb", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_actors", x => x.guid);
            });

        migrationBuilder.CreateTable(
            name: "actor_partition_accesses",
            columns: table => new
            {
                actor_guid = table.Column<Guid>(type: "uuid", nullable: false),
                partition_guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_actor_partition_accesses", x => new { x.actor_guid, x.partition_guid });
                table.ForeignKey(
                    name: "fk_actor_partition_accesses_actors_actor_guid",
                    column: x => x.actor_guid,
                    principalTable: "actors",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_actor_partition_accesses_partitions_partition_guid",
                    column: x => x.partition_guid,
                    principalTable: "partitions",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "actor_partitions",
            columns: table => new
            {
                actor_guid = table.Column<Guid>(type: "uuid", nullable: false),
                partition_guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_actor_partitions", x => new { x.actor_guid, x.partition_guid });
                table.ForeignKey(
                    name: "fk_actor_partitions_actors_actor_guid",
                    column: x => x.actor_guid,
                    principalTable: "actors",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_actor_partitions_partitions_partition_guid",
                    column: x => x.partition_guid,
                    principalTable: "partitions",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_actor_partition_accesses_partition_guid",
            table: "actor_partition_accesses",
            column: "partition_guid");

        migrationBuilder.CreateIndex(
            name: "ix_actor_partitions_partition_guid",
            table: "actor_partitions",
            column: "partition_guid");

        migrationBuilder.AddForeignKey(
            name: "fk_service_accounts_actors_actor_guid",
            table: "service_accounts",
            column: "actor_guid",
            principalTable: "actors",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_users_actors_actor_guid",
            table: "users",
            column: "actor_guid",
            principalTable: "actors",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_service_accounts_actors_actor_guid",
            table: "service_accounts");

        migrationBuilder.DropForeignKey(
            name: "fk_users_actors_actor_guid",
            table: "users");

        migrationBuilder.DropTable(
            name: "actor_partition_accesses");

        migrationBuilder.DropTable(
            name: "actor_partitions");

        migrationBuilder.DropTable(
            name: "actors");

        migrationBuilder.RenameColumn(
            name: "actor_guid",
            table: "users",
            newName: "guid");

        migrationBuilder.RenameColumn(
            name: "actor_guid",
            table: "service_accounts",
            newName: "guid");

        migrationBuilder.AddColumn<string>(
            name: "description",
            table: "users",
            type: "character varying(500)",
            maxLength: 500,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<bool>(
            name: "is_active",
            table: "users",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "permissions",
            table: "users",
            type: "jsonb",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<uint>(
            name: "xmin",
            table: "users",
            type: "xid",
            rowVersion: true,
            nullable: false,
            defaultValue: 0u);

        migrationBuilder.AddColumn<string>(
            name: "description",
            table: "service_accounts",
            type: "character varying(500)",
            maxLength: 500,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<bool>(
            name: "is_active",
            table: "service_accounts",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "partition_access_guids",
            table: "service_accounts",
            type: "jsonb",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "permissions",
            table: "service_accounts",
            type: "jsonb",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<uint>(
            name: "xmin",
            table: "service_accounts",
            type: "xid",
            rowVersion: true,
            nullable: false,
            defaultValue: 0u);

        migrationBuilder.CreateTable(
            name: "service_account_partitions",
            columns: table => new
            {
                service_account_guid = table.Column<Guid>(type: "uuid", nullable: false),
                partition_guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_service_account_partitions", x => new { x.service_account_guid, x.partition_guid });
                table.ForeignKey(
                    name: "fk_service_account_partitions_partitions_partition_guid",
                    column: x => x.partition_guid,
                    principalTable: "partitions",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_service_account_partitions_service_accounts_service_account",
                    column: x => x.service_account_guid,
                    principalTable: "service_accounts",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_partition_accesses",
            columns: table => new
            {
                user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                partition_guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_partition_accesses", x => new { x.user_guid, x.partition_guid });
                table.ForeignKey(
                    name: "fk_user_partition_accesses_partitions_partition_guid",
                    column: x => x.partition_guid,
                    principalTable: "partitions",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_user_partition_accesses_users_user_guid",
                    column: x => x.user_guid,
                    principalTable: "users",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_partitions",
            columns: table => new
            {
                user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                partition_guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_partitions", x => new { x.user_guid, x.partition_guid });
                table.ForeignKey(
                    name: "fk_user_partitions_partitions_partition_guid",
                    column: x => x.partition_guid,
                    principalTable: "partitions",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_user_partitions_users_user_guid",
                    column: x => x.user_guid,
                    principalTable: "users",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_service_account_partitions_partition_guid",
            table: "service_account_partitions",
            column: "partition_guid");

        migrationBuilder.CreateIndex(
            name: "ix_user_partition_accesses_partition_guid",
            table: "user_partition_accesses",
            column: "partition_guid");

        migrationBuilder.CreateIndex(
            name: "ix_user_partitions_partition_guid",
            table: "user_partitions",
            column: "partition_guid");
    }
}
