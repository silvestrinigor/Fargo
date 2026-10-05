using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class NewMigrartion4 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_article_partitions_partitions_partition_guid",
            table: "article_partitions");

        migrationBuilder.DropForeignKey(
            name: "fk_item_partitions_partitions_partition_guid",
            table: "item_partitions");

        migrationBuilder.DropForeignKey(
            name: "fk_user_group_partitions_partitions_partition_guid",
            table: "user_group_partitions");

        migrationBuilder.DropForeignKey(
            name: "fk_user_partitions_partitions_partition_guid",
            table: "user_partitions");

        migrationBuilder.AddForeignKey(
            name: "fk_article_partitions_partitions_partition_guid",
            table: "article_partitions",
            column: "partition_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "fk_item_partitions_partitions_partition_guid",
            table: "item_partitions",
            column: "partition_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "fk_user_group_partitions_partitions_partition_guid",
            table: "user_group_partitions",
            column: "partition_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "fk_user_partitions_partitions_partition_guid",
            table: "user_partitions",
            column: "partition_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Restrict);
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
            name: "fk_user_group_partitions_partitions_partition_guid",
            table: "user_group_partitions");

        migrationBuilder.DropForeignKey(
            name: "fk_user_partitions_partitions_partition_guid",
            table: "user_partitions");

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
            name: "fk_user_group_partitions_partitions_partition_guid",
            table: "user_group_partitions",
            column: "partition_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_partitions_partitions_partition_guid",
            table: "user_partitions",
            column: "partition_guid",
            principalTable: "partitions",
            principalColumn: "guid",
            onDelete: ReferentialAction.Cascade);
    }
}
