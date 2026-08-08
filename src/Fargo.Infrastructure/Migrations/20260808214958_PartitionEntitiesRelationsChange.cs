using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class PartitionEntitiesRelationsChange : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_article_partitions_partitions_partitions_guid",
            table: "article_partitions");

        migrationBuilder.DropForeignKey(
            name: "fk_item_partitions_partitions_partitions_guid",
            table: "item_partitions");

        migrationBuilder.DropForeignKey(
            name: "fk_user_group_partition_accesses_partitions_partition_accesses",
            table: "user_group_partition_accesses");

        migrationBuilder.DropForeignKey(
            name: "fk_user_group_partition_accesses_user_groups_user_group1guid",
            table: "user_group_partition_accesses");

        migrationBuilder.DropForeignKey(
            name: "fk_user_group_partitions_partitions_partitions_guid",
            table: "user_group_partitions");

        migrationBuilder.DropForeignKey(
            name: "fk_user_partition_accesses_partitions_partition_accesses_guid",
            table: "user_partition_accesses");

        migrationBuilder.DropForeignKey(
            name: "fk_user_partition_accesses_users_user1guid",
            table: "user_partition_accesses");

        migrationBuilder.DropForeignKey(
            name: "fk_user_partitions_partitions_partitions_guid",
            table: "user_partitions");

        migrationBuilder.DropForeignKey(
            name: "fk_user_user_groups_user_groups_user_groups_guid",
            table: "user_user_groups");

        migrationBuilder.DropPrimaryKey(
            name: "pk_user_user_groups",
            table: "user_user_groups");

        migrationBuilder.DropIndex(
            name: "ix_user_user_groups_user_guid",
            table: "user_user_groups");

        migrationBuilder.DropPrimaryKey(
            name: "pk_user_partitions",
            table: "user_partitions");

        migrationBuilder.DropIndex(
            name: "ix_user_partitions_user_guid",
            table: "user_partitions");

        migrationBuilder.DropPrimaryKey(
            name: "pk_user_group_partitions",
            table: "user_group_partitions");

        migrationBuilder.DropIndex(
            name: "ix_user_group_partitions_user_group_guid",
            table: "user_group_partitions");

        migrationBuilder.RenameColumn(
            name: "user_groups_guid",
            table: "user_user_groups",
            newName: "user_group_guid");

        migrationBuilder.RenameColumn(
            name: "partitions_guid",
            table: "user_partitions",
            newName: "partition_guid");

        migrationBuilder.RenameColumn(
            name: "user1guid",
            table: "user_partition_accesses",
            newName: "partition_guid");

        migrationBuilder.RenameColumn(
            name: "partition_accesses_guid",
            table: "user_partition_accesses",
            newName: "user_guid");

        migrationBuilder.RenameIndex(
            name: "ix_user_partition_accesses_user1guid",
            table: "user_partition_accesses",
            newName: "ix_user_partition_accesses_partition_guid");

        migrationBuilder.RenameColumn(
            name: "partitions_guid",
            table: "user_group_partitions",
            newName: "partition_guid");

        migrationBuilder.RenameColumn(
            name: "user_group1guid",
            table: "user_group_partition_accesses",
            newName: "partition_guid");

        migrationBuilder.RenameColumn(
            name: "partition_accesses_guid",
            table: "user_group_partition_accesses",
            newName: "user_group_guid");

        migrationBuilder.RenameIndex(
            name: "ix_user_group_partition_accesses_user_group1guid",
            table: "user_group_partition_accesses",
            newName: "ix_user_group_partition_accesses_partition_guid");

        migrationBuilder.RenameColumn(
            name: "partitions_guid",
            table: "item_partitions",
            newName: "partition_guid");

        migrationBuilder.RenameIndex(
            name: "ix_item_partitions_partitions_guid",
            table: "item_partitions",
            newName: "ix_item_partitions_partition_guid");

        migrationBuilder.RenameColumn(
            name: "partitions_guid",
            table: "article_partitions",
            newName: "partition_guid");

        migrationBuilder.RenameIndex(
            name: "ix_article_partitions_partitions_guid",
            table: "article_partitions",
            newName: "ix_article_partitions_partition_guid");

        migrationBuilder.AddPrimaryKey(
            name: "pk_user_user_groups",
            table: "user_user_groups",
            columns: new[] { "user_guid", "user_group_guid" });

        migrationBuilder.AddPrimaryKey(
            name: "pk_user_partitions",
            table: "user_partitions",
            columns: new[] { "user_guid", "partition_guid" });

        migrationBuilder.AddPrimaryKey(
            name: "pk_user_group_partitions",
            table: "user_group_partitions",
            columns: new[] { "user_group_guid", "partition_guid" });

        migrationBuilder.CreateIndex(
            name: "ix_user_user_groups_user_group_guid",
            table: "user_user_groups",
            column: "user_group_guid");

        migrationBuilder.CreateIndex(
            name: "ix_user_partitions_partition_guid",
            table: "user_partitions",
            column: "partition_guid");

        migrationBuilder.CreateIndex(
            name: "ix_user_group_partitions_partition_guid",
            table: "user_group_partitions",
            column: "partition_guid");

        migrationBuilder.AddForeignKey(
            name: "fk_article_partitions_partitions_partition_guid",
            table: "article_partitions",
            column: "partition_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_item_partitions_partitions_partition_guid",
            table: "item_partitions",
            column: "partition_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_group_partition_accesses_partitions_partition_guid",
            table: "user_group_partition_accesses",
            column: "partition_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_group_partition_accesses_user_groups_user_group_guid",
            table: "user_group_partition_accesses",
            column: "user_group_guid",
            principalTable: "user_groups",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_group_partitions_partitions_partition_guid",
            table: "user_group_partitions",
            column: "partition_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_partition_accesses_partitions_partition_guid",
            table: "user_partition_accesses",
            column: "partition_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_partition_accesses_users_user_guid",
            table: "user_partition_accesses",
            column: "user_guid",
            principalTable: "users",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_partitions_partitions_partition_guid",
            table: "user_partitions",
            column: "partition_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_user_groups_user_groups_user_group_guid",
            table: "user_user_groups",
            column: "user_group_guid",
            principalTable: "user_groups",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_article_partitions_partitions_partition_guid",
            table: "article_partitions");

        migrationBuilder.DropForeignKey(
            name: "fk_item_partitions_partitions_partition_guid",
            table: "item_partitions");

        migrationBuilder.DropForeignKey(
            name: "fk_user_group_partition_accesses_partitions_partition_guid",
            table: "user_group_partition_accesses");

        migrationBuilder.DropForeignKey(
            name: "fk_user_group_partition_accesses_user_groups_user_group_guid",
            table: "user_group_partition_accesses");

        migrationBuilder.DropForeignKey(
            name: "fk_user_group_partitions_partitions_partition_guid",
            table: "user_group_partitions");

        migrationBuilder.DropForeignKey(
            name: "fk_user_partition_accesses_partitions_partition_guid",
            table: "user_partition_accesses");

        migrationBuilder.DropForeignKey(
            name: "fk_user_partition_accesses_users_user_guid",
            table: "user_partition_accesses");

        migrationBuilder.DropForeignKey(
            name: "fk_user_partitions_partitions_partition_guid",
            table: "user_partitions");

        migrationBuilder.DropForeignKey(
            name: "fk_user_user_groups_user_groups_user_group_guid",
            table: "user_user_groups");

        migrationBuilder.DropPrimaryKey(
            name: "pk_user_user_groups",
            table: "user_user_groups");

        migrationBuilder.DropIndex(
            name: "ix_user_user_groups_user_group_guid",
            table: "user_user_groups");

        migrationBuilder.DropPrimaryKey(
            name: "pk_user_partitions",
            table: "user_partitions");

        migrationBuilder.DropIndex(
            name: "ix_user_partitions_partition_guid",
            table: "user_partitions");

        migrationBuilder.DropPrimaryKey(
            name: "pk_user_group_partitions",
            table: "user_group_partitions");

        migrationBuilder.DropIndex(
            name: "ix_user_group_partitions_partition_guid",
            table: "user_group_partitions");

        migrationBuilder.RenameColumn(
            name: "user_group_guid",
            table: "user_user_groups",
            newName: "user_groups_guid");

        migrationBuilder.RenameColumn(
            name: "partition_guid",
            table: "user_partitions",
            newName: "partitions_guid");

        migrationBuilder.RenameColumn(
            name: "partition_guid",
            table: "user_partition_accesses",
            newName: "user1guid");

        migrationBuilder.RenameColumn(
            name: "user_guid",
            table: "user_partition_accesses",
            newName: "partition_accesses_guid");

        migrationBuilder.RenameIndex(
            name: "ix_user_partition_accesses_partition_guid",
            table: "user_partition_accesses",
            newName: "ix_user_partition_accesses_user1guid");

        migrationBuilder.RenameColumn(
            name: "partition_guid",
            table: "user_group_partitions",
            newName: "partitions_guid");

        migrationBuilder.RenameColumn(
            name: "partition_guid",
            table: "user_group_partition_accesses",
            newName: "user_group1guid");

        migrationBuilder.RenameColumn(
            name: "user_group_guid",
            table: "user_group_partition_accesses",
            newName: "partition_accesses_guid");

        migrationBuilder.RenameIndex(
            name: "ix_user_group_partition_accesses_partition_guid",
            table: "user_group_partition_accesses",
            newName: "ix_user_group_partition_accesses_user_group1guid");

        migrationBuilder.RenameColumn(
            name: "partition_guid",
            table: "item_partitions",
            newName: "partitions_guid");

        migrationBuilder.RenameIndex(
            name: "ix_item_partitions_partition_guid",
            table: "item_partitions",
            newName: "ix_item_partitions_partitions_guid");

        migrationBuilder.RenameColumn(
            name: "partition_guid",
            table: "article_partitions",
            newName: "partitions_guid");

        migrationBuilder.RenameIndex(
            name: "ix_article_partitions_partition_guid",
            table: "article_partitions",
            newName: "ix_article_partitions_partitions_guid");

        migrationBuilder.AddPrimaryKey(
            name: "pk_user_user_groups",
            table: "user_user_groups",
            columns: new[] { "user_groups_guid", "user_guid" });

        migrationBuilder.AddPrimaryKey(
            name: "pk_user_partitions",
            table: "user_partitions",
            columns: new[] { "partitions_guid", "user_guid" });

        migrationBuilder.AddPrimaryKey(
            name: "pk_user_group_partitions",
            table: "user_group_partitions",
            columns: new[] { "partitions_guid", "user_group_guid" });

        migrationBuilder.CreateIndex(
            name: "ix_user_user_groups_user_guid",
            table: "user_user_groups",
            column: "user_guid");

        migrationBuilder.CreateIndex(
            name: "ix_user_partitions_user_guid",
            table: "user_partitions",
            column: "user_guid");

        migrationBuilder.CreateIndex(
            name: "ix_user_group_partitions_user_group_guid",
            table: "user_group_partitions",
            column: "user_group_guid");

        migrationBuilder.AddForeignKey(
            name: "fk_article_partitions_partitions_partitions_guid",
            table: "article_partitions",
            column: "partitions_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_item_partitions_partitions_partitions_guid",
            table: "item_partitions",
            column: "partitions_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_group_partition_accesses_partitions_partition_accesses",
            table: "user_group_partition_accesses",
            column: "partition_accesses_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_group_partition_accesses_user_groups_user_group1guid",
            table: "user_group_partition_accesses",
            column: "user_group1guid",
            principalTable: "user_groups",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_group_partitions_partitions_partitions_guid",
            table: "user_group_partitions",
            column: "partitions_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_partition_accesses_partitions_partition_accesses_guid",
            table: "user_partition_accesses",
            column: "partition_accesses_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_partition_accesses_users_user1guid",
            table: "user_partition_accesses",
            column: "user1guid",
            principalTable: "users",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_partitions_partitions_partitions_guid",
            table: "user_partitions",
            column: "partitions_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_user_groups_user_groups_user_groups_guid",
            table: "user_user_groups",
            column: "user_groups_guid",
            principalTable: "user_groups",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);
    }
}
