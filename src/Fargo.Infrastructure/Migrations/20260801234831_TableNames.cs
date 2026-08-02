using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_article_partition_articles_article_guid",
                table: "article_partition");

            migrationBuilder.DropForeignKey(
                name: "fk_article_partition_partitions_partitions_guid",
                table: "article_partition");

            migrationBuilder.DropForeignKey(
                name: "fk_item_partition_items_item_guid",
                table: "item_partition");

            migrationBuilder.DropForeignKey(
                name: "fk_item_partition_partitions_partitions_guid",
                table: "item_partition");

            migrationBuilder.DropForeignKey(
                name: "fk_partition_user_partitions_partitions_guid",
                table: "partition_user");

            migrationBuilder.DropForeignKey(
                name: "fk_partition_user_users_user_guid",
                table: "partition_user");

            migrationBuilder.DropForeignKey(
                name: "fk_partition_user_group_partitions_partitions_guid",
                table: "partition_user_group");

            migrationBuilder.DropForeignKey(
                name: "fk_partition_user_group_user_groups_user_group_guid",
                table: "partition_user_group");

            migrationBuilder.DropForeignKey(
                name: "fk_partition_user1_partitions_partition_accesses_guid",
                table: "partition_user1");

            migrationBuilder.DropForeignKey(
                name: "fk_partition_user1_users_user1guid",
                table: "partition_user1");

            migrationBuilder.DropForeignKey(
                name: "fk_user_user_group_user_groups_user_groups_guid",
                table: "user_user_group");

            migrationBuilder.DropForeignKey(
                name: "fk_user_user_group_users_user_guid",
                table: "user_user_group");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_user_group",
                table: "user_user_group");

            migrationBuilder.DropPrimaryKey(
                name: "pk_partition_user1",
                table: "partition_user1");

            migrationBuilder.DropPrimaryKey(
                name: "pk_partition_user_group",
                table: "partition_user_group");

            migrationBuilder.DropPrimaryKey(
                name: "pk_partition_user",
                table: "partition_user");

            migrationBuilder.DropPrimaryKey(
                name: "pk_item_partition",
                table: "item_partition");

            migrationBuilder.DropPrimaryKey(
                name: "pk_article_partition",
                table: "article_partition");

            migrationBuilder.RenameTable(
                name: "user_user_group",
                newName: "user_user_groups");

            migrationBuilder.RenameTable(
                name: "partition_user1",
                newName: "user_partition_accesses");

            migrationBuilder.RenameTable(
                name: "partition_user_group",
                newName: "user_group_partitions");

            migrationBuilder.RenameTable(
                name: "partition_user",
                newName: "user_partitions");

            migrationBuilder.RenameTable(
                name: "item_partition",
                newName: "item_partitions");

            migrationBuilder.RenameTable(
                name: "article_partition",
                newName: "article_partitions");

            migrationBuilder.RenameIndex(
                name: "ix_user_user_group_user_guid",
                table: "user_user_groups",
                newName: "ix_user_user_groups_user_guid");

            migrationBuilder.RenameIndex(
                name: "ix_partition_user1_user1guid",
                table: "user_partition_accesses",
                newName: "ix_user_partition_accesses_user1guid");

            migrationBuilder.RenameIndex(
                name: "ix_partition_user_group_user_group_guid",
                table: "user_group_partitions",
                newName: "ix_user_group_partitions_user_group_guid");

            migrationBuilder.RenameIndex(
                name: "ix_partition_user_user_guid",
                table: "user_partitions",
                newName: "ix_user_partitions_user_guid");

            migrationBuilder.RenameIndex(
                name: "ix_item_partition_partitions_guid",
                table: "item_partitions",
                newName: "ix_item_partitions_partitions_guid");

            migrationBuilder.RenameIndex(
                name: "ix_article_partition_partitions_guid",
                table: "article_partitions",
                newName: "ix_article_partitions_partitions_guid");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_user_groups",
                table: "user_user_groups",
                columns: new[] { "user_groups_guid", "user_guid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_partition_accesses",
                table: "user_partition_accesses",
                columns: new[] { "partition_accesses_guid", "user1guid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_group_partitions",
                table: "user_group_partitions",
                columns: new[] { "partitions_guid", "user_group_guid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_partitions",
                table: "user_partitions",
                columns: new[] { "partitions_guid", "user_guid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_item_partitions",
                table: "item_partitions",
                columns: new[] { "item_guid", "partitions_guid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_article_partitions",
                table: "article_partitions",
                columns: new[] { "article_guid", "partitions_guid" });

            migrationBuilder.AddForeignKey(
                name: "fk_article_partitions_articles_article_guid",
                table: "article_partitions",
                column: "article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_article_partitions_partitions_partitions_guid",
                table: "article_partitions",
                column: "partitions_guid",
                principalTable: "partitions",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_item_partitions_items_item_guid",
                table: "item_partitions",
                column: "item_guid",
                principalTable: "items",
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
                name: "fk_user_group_partitions_partitions_partitions_guid",
                table: "user_group_partitions",
                column: "partitions_guid",
                principalTable: "partitions",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_group_partitions_user_groups_user_group_guid",
                table: "user_group_partitions",
                column: "user_group_guid",
                principalTable: "user_groups",
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
                name: "fk_user_partitions_users_user_guid",
                table: "user_partitions",
                column: "user_guid",
                principalTable: "users",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_user_groups_user_groups_user_groups_guid",
                table: "user_user_groups",
                column: "user_groups_guid",
                principalTable: "user_groups",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_user_groups_users_user_guid",
                table: "user_user_groups",
                column: "user_guid",
                principalTable: "users",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_article_partitions_articles_article_guid",
                table: "article_partitions");

            migrationBuilder.DropForeignKey(
                name: "fk_article_partitions_partitions_partitions_guid",
                table: "article_partitions");

            migrationBuilder.DropForeignKey(
                name: "fk_item_partitions_items_item_guid",
                table: "item_partitions");

            migrationBuilder.DropForeignKey(
                name: "fk_item_partitions_partitions_partitions_guid",
                table: "item_partitions");

            migrationBuilder.DropForeignKey(
                name: "fk_user_group_partitions_partitions_partitions_guid",
                table: "user_group_partitions");

            migrationBuilder.DropForeignKey(
                name: "fk_user_group_partitions_user_groups_user_group_guid",
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
                name: "fk_user_partitions_users_user_guid",
                table: "user_partitions");

            migrationBuilder.DropForeignKey(
                name: "fk_user_user_groups_user_groups_user_groups_guid",
                table: "user_user_groups");

            migrationBuilder.DropForeignKey(
                name: "fk_user_user_groups_users_user_guid",
                table: "user_user_groups");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_user_groups",
                table: "user_user_groups");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_partitions",
                table: "user_partitions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_partition_accesses",
                table: "user_partition_accesses");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_group_partitions",
                table: "user_group_partitions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_item_partitions",
                table: "item_partitions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_article_partitions",
                table: "article_partitions");

            migrationBuilder.RenameTable(
                name: "user_user_groups",
                newName: "user_user_group");

            migrationBuilder.RenameTable(
                name: "user_partitions",
                newName: "partition_user");

            migrationBuilder.RenameTable(
                name: "user_partition_accesses",
                newName: "partition_user1");

            migrationBuilder.RenameTable(
                name: "user_group_partitions",
                newName: "partition_user_group");

            migrationBuilder.RenameTable(
                name: "item_partitions",
                newName: "item_partition");

            migrationBuilder.RenameTable(
                name: "article_partitions",
                newName: "article_partition");

            migrationBuilder.RenameIndex(
                name: "ix_user_user_groups_user_guid",
                table: "user_user_group",
                newName: "ix_user_user_group_user_guid");

            migrationBuilder.RenameIndex(
                name: "ix_user_partitions_user_guid",
                table: "partition_user",
                newName: "ix_partition_user_user_guid");

            migrationBuilder.RenameIndex(
                name: "ix_user_partition_accesses_user1guid",
                table: "partition_user1",
                newName: "ix_partition_user1_user1guid");

            migrationBuilder.RenameIndex(
                name: "ix_user_group_partitions_user_group_guid",
                table: "partition_user_group",
                newName: "ix_partition_user_group_user_group_guid");

            migrationBuilder.RenameIndex(
                name: "ix_item_partitions_partitions_guid",
                table: "item_partition",
                newName: "ix_item_partition_partitions_guid");

            migrationBuilder.RenameIndex(
                name: "ix_article_partitions_partitions_guid",
                table: "article_partition",
                newName: "ix_article_partition_partitions_guid");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_user_group",
                table: "user_user_group",
                columns: new[] { "user_groups_guid", "user_guid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_partition_user",
                table: "partition_user",
                columns: new[] { "partitions_guid", "user_guid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_partition_user1",
                table: "partition_user1",
                columns: new[] { "partition_accesses_guid", "user1guid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_partition_user_group",
                table: "partition_user_group",
                columns: new[] { "partitions_guid", "user_group_guid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_item_partition",
                table: "item_partition",
                columns: new[] { "item_guid", "partitions_guid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_article_partition",
                table: "article_partition",
                columns: new[] { "article_guid", "partitions_guid" });

            migrationBuilder.AddForeignKey(
                name: "fk_article_partition_articles_article_guid",
                table: "article_partition",
                column: "article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_article_partition_partitions_partitions_guid",
                table: "article_partition",
                column: "partitions_guid",
                principalTable: "partitions",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_item_partition_items_item_guid",
                table: "item_partition",
                column: "item_guid",
                principalTable: "items",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_item_partition_partitions_partitions_guid",
                table: "item_partition",
                column: "partitions_guid",
                principalTable: "partitions",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_partition_user_partitions_partitions_guid",
                table: "partition_user",
                column: "partitions_guid",
                principalTable: "partitions",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_partition_user_users_user_guid",
                table: "partition_user",
                column: "user_guid",
                principalTable: "users",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_partition_user_group_partitions_partitions_guid",
                table: "partition_user_group",
                column: "partitions_guid",
                principalTable: "partitions",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_partition_user_group_user_groups_user_group_guid",
                table: "partition_user_group",
                column: "user_group_guid",
                principalTable: "user_groups",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_partition_user1_partitions_partition_accesses_guid",
                table: "partition_user1",
                column: "partition_accesses_guid",
                principalTable: "partitions",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_partition_user1_users_user1guid",
                table: "partition_user1",
                column: "user1guid",
                principalTable: "users",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_user_group_user_groups_user_groups_guid",
                table: "user_user_group",
                column: "user_groups_guid",
                principalTable: "user_groups",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_user_group_users_user_guid",
                table: "user_user_group",
                column: "user_guid",
                principalTable: "users",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
