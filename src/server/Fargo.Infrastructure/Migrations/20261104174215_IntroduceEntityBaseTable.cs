using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class IntroduceEntityBaseTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "pk_user_groups",
            table: "user_groups");

        migrationBuilder.DropPrimaryKey(
            name: "pk_partitions",
            table: "partitions");

        migrationBuilder.DropPrimaryKey(
            name: "pk_items",
            table: "items");

        migrationBuilder.DropPrimaryKey(
            name: "pk_articles",
            table: "articles");

        migrationBuilder.DropPrimaryKey(
            name: "pk_actors",
            table: "actors");

        migrationBuilder.AddPrimaryKey(
            name: "pk_entities",
            table: "user_groups",
            column: "guid");

        migrationBuilder.AddPrimaryKey(
            name: "pk_entities",
            table: "partitions",
            column: "guid");

        migrationBuilder.AddPrimaryKey(
            name: "pk_entities",
            table: "items",
            column: "guid");

        migrationBuilder.AddPrimaryKey(
            name: "pk_entities",
            table: "articles",
            column: "guid");

        migrationBuilder.AddPrimaryKey(
            name: "pk_entities",
            table: "actors",
            column: "guid");

        migrationBuilder.CreateTable(
            name: "entities",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                entity_type = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_entities", x => x.guid);
            });

        migrationBuilder.CreateIndex(
            name: "ix_entities_entity_type",
            table: "entities",
            column: "entity_type");

        migrationBuilder.AddForeignKey(
            name: "fk_actors_entities_guid",
            table: "actors",
            column: "guid",
            principalTable: "entities",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_articles_entities_guid",
            table: "articles",
            column: "guid",
            principalTable: "entities",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_items_entities_guid",
            table: "items",
            column: "guid",
            principalTable: "entities",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_partitions_entities_guid",
            table: "partitions",
            column: "guid",
            principalTable: "entities",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_groups_entities_guid",
            table: "user_groups",
            column: "guid",
            principalTable: "entities",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_actors_entities_guid",
            table: "actors");

        migrationBuilder.DropForeignKey(
            name: "fk_articles_entities_guid",
            table: "articles");

        migrationBuilder.DropForeignKey(
            name: "fk_items_entities_guid",
            table: "items");

        migrationBuilder.DropForeignKey(
            name: "fk_partitions_entities_guid",
            table: "partitions");

        migrationBuilder.DropForeignKey(
            name: "fk_user_groups_entities_guid",
            table: "user_groups");

        migrationBuilder.DropTable(
            name: "entities");

        migrationBuilder.DropPrimaryKey(
            name: "pk_entities",
            table: "user_groups");

        migrationBuilder.DropPrimaryKey(
            name: "pk_entities",
            table: "partitions");

        migrationBuilder.DropPrimaryKey(
            name: "pk_entities",
            table: "items");

        migrationBuilder.DropPrimaryKey(
            name: "pk_entities",
            table: "articles");

        migrationBuilder.DropPrimaryKey(
            name: "pk_entities",
            table: "actors");

        migrationBuilder.AddPrimaryKey(
            name: "pk_user_groups",
            table: "user_groups",
            column: "guid");

        migrationBuilder.AddPrimaryKey(
            name: "pk_partitions",
            table: "partitions",
            column: "guid");

        migrationBuilder.AddPrimaryKey(
            name: "pk_items",
            table: "items",
            column: "guid");

        migrationBuilder.AddPrimaryKey(
            name: "pk_articles",
            table: "articles",
            column: "guid");

        migrationBuilder.AddPrimaryKey(
            name: "pk_actors",
            table: "actors",
            column: "guid");
    }
}
