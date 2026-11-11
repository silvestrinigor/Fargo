using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class IntroduceEntityBaseTable : Migration
{
    // Derived TPT tables keep their own primary key; only the constraint is
    // renamed to the EF default name for a TPT table ("PK_<table>"). The keys
    // are renamed instead of dropped and recreated because other tables hold
    // foreign keys that depend on them (PostgreSQL error 2BP01 otherwise).
    //
    // Values of entity_type mirror Fargo.Core.Entities.EntityType.
    private static readonly (string Table, string OldKey, int EntityType)[] DerivedTables =
    [
        ("articles", "pk_articles", 0),
        ("items", "pk_items", 1),
        ("actors", "pk_actors", 2),
        ("user_groups", "pk_user_groups", 3),
        ("partitions", "pk_partitions", 4),
    ];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var (table, oldKey, _) in DerivedTables)
        {
            migrationBuilder.Sql($"ALTER TABLE {table} RENAME CONSTRAINT {oldKey} TO \"PK_{table}\";");
        }

        migrationBuilder.CreateTable(
            name: "entities",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                entity_type = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_entities", x => x.guid);
            });

        migrationBuilder.CreateIndex(
            name: "ix_entities_entity_type",
            table: "entities",
            column: "entity_type");

        // Backfill the base rows so the new foreign keys hold for existing data.
        foreach (var (table, _, entityType) in DerivedTables)
        {
            migrationBuilder.Sql($"INSERT INTO entities (guid, entity_type) SELECT guid, {entityType} FROM {table};");
        }

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

        foreach (var (table, oldKey, _) in DerivedTables)
        {
            migrationBuilder.Sql($"ALTER TABLE {table} RENAME CONSTRAINT \"PK_{table}\" TO {oldKey};");
        }
    }
}
