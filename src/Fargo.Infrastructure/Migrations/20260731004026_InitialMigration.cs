using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "article_containers",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    max_mass = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_containers", x => x.guid);
                });

            migrationBuilder.CreateTable(
                name: "article_kits",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_kits", x => x.guid);
                });

            migrationBuilder.CreateTable(
                name: "item_containers",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_containers", x => x.guid);
                });

            migrationBuilder.CreateTable(
                name: "user_groups",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    nameid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_groups", x => x.guid);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    nameid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    default_password_expiration_period = table.Column<long>(type: "bigint", nullable: true),
                    require_password_change_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    auth_version = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.guid);
                });

            migrationBuilder.CreateTable(
                name: "partitions",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    parent_partition_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    user_group_guid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_partitions", x => x.guid);
                    table.ForeignKey(
                        name: "fk_partitions_partitions_parent_partition_guid",
                        column: x => x.parent_partition_guid,
                        principalTable: "partitions",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_partitions_user_groups_user_group_guid",
                        column: x => x.user_group_guid,
                        principalTable: "user_groups",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "user_group_permissions",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_group_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_group_permissions", x => x.guid);
                    table.UniqueConstraint("ak_user_group_permissions_user_group_guid_action", x => new { x.user_group_guid, x.action });
                    table.ForeignKey(
                        name: "fk_user_group_permissions_user_groups_user_group_guid",
                        column: x => x.user_group_guid,
                        principalTable: "user_groups",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.guid);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_guid",
                        column: x => x.user_guid,
                        principalTable: "users",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_user_group",
                columns: table => new
                {
                    user_groups_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_user_group", x => new { x.user_groups_guid, x.user_guid });
                    table.ForeignKey(
                        name: "fk_user_user_group_user_groups_user_groups_guid",
                        column: x => x.user_groups_guid,
                        principalTable: "user_groups",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_user_group_users_user_guid",
                        column: x => x.user_guid,
                        principalTable: "users",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "partition_user",
                columns: table => new
                {
                    partitions_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_partition_user", x => new { x.partitions_guid, x.user_guid });
                    table.ForeignKey(
                        name: "fk_partition_user_partitions_partitions_guid",
                        column: x => x.partitions_guid,
                        principalTable: "partitions",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_partition_user_users_user_guid",
                        column: x => x.user_guid,
                        principalTable: "users",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "partition_user_group",
                columns: table => new
                {
                    partitions_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_group_guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_partition_user_group", x => new { x.partitions_guid, x.user_group_guid });
                    table.ForeignKey(
                        name: "fk_partition_user_group_partitions_partitions_guid",
                        column: x => x.partitions_guid,
                        principalTable: "partitions",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_partition_user_group_user_groups_user_group_guid",
                        column: x => x.user_group_guid,
                        principalTable: "user_groups",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "partition_user1",
                columns: table => new
                {
                    partition_accesses_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user1guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_partition_user1", x => new { x.partition_accesses_guid, x.user1guid });
                    table.ForeignKey(
                        name: "fk_partition_user1_partitions_partition_accesses_guid",
                        column: x => x.partition_accesses_guid,
                        principalTable: "partitions",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_partition_user1_users_user1guid",
                        column: x => x.user1guid,
                        principalTable: "users",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "article_kit_components",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<double>(type: "double precision", nullable: false),
                    article_kit_guid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_kit_components", x => x.guid);
                    table.ForeignKey(
                        name: "fk_article_kit_components_article_kits_article_kit_guid",
                        column: x => x.article_kit_guid,
                        principalTable: "article_kits",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "article_packs",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    from_article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_packs", x => x.guid);
                });

            migrationBuilder.CreateTable(
                name: "article_partition",
                columns: table => new
                {
                    article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    partitions_guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_partition", x => new { x.article_guid, x.partitions_guid });
                    table.ForeignKey(
                        name: "fk_article_partition_partitions_partitions_guid",
                        column: x => x.partitions_guid,
                        principalTable: "partitions",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "article_variations",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    from_article_guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_variations", x => x.guid);
                });

            migrationBuilder.CreateTable(
                name: "articles",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    article_type = table.Column<int>(type: "integer", nullable: false),
                    shelf_life = table.Column<long>(type: "bigint", nullable: true),
                    color = table.Column<int>(type: "integer", nullable: true),
                    length_x = table.Column<string>(type: "text", nullable: true),
                    length_y = table.Column<string>(type: "text", nullable: true),
                    length_z = table.Column<string>(type: "text", nullable: true),
                    mass = table.Column<string>(type: "text", nullable: true),
                    ean13 = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    ean8 = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    upc_a = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    upc_e = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    code128 = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    code39 = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    itf14 = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    gs1128 = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    qr_code = table.Column<string>(type: "character varying(2953)", maxLength: 2953, nullable: true),
                    data_matrix = table.Column<string>(type: "character varying(2335)", maxLength: 2335, nullable: true),
                    article_guid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_articles", x => x.guid);
                    table.ForeignKey(
                        name: "fk_articles_article_containers_article_guid",
                        column: x => x.article_guid,
                        principalTable: "article_containers",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_articles_article_kits_article_guid",
                        column: x => x.article_guid,
                        principalTable: "article_kits",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_articles_article_packs_article_guid",
                        column: x => x.article_guid,
                        principalTable: "article_packs",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_articles_article_variations_article_guid",
                        column: x => x.article_guid,
                        principalTable: "article_variations",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    production_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    parent_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    article_guid1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_items", x => x.guid);
                    table.ForeignKey(
                        name: "fk_items_articles_article_guid",
                        column: x => x.article_guid,
                        principalTable: "articles",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_items_item_containers_article_guid",
                        column: x => x.article_guid1,
                        principalTable: "item_containers",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_items_items_parent_container_guid",
                        column: x => x.parent_container_guid,
                        principalTable: "items",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "item_partition",
                columns: table => new
                {
                    item_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    partitions_guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_partition", x => new { x.item_guid, x.partitions_guid });
                    table.ForeignKey(
                        name: "fk_item_partition_items_item_guid",
                        column: x => x.item_guid,
                        principalTable: "items",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_item_partition_partitions_partitions_guid",
                        column: x => x.partitions_guid,
                        principalTable: "partitions",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_article_kit_components_article_guid",
                table: "article_kit_components",
                column: "article_guid");

            migrationBuilder.CreateIndex(
                name: "ix_article_kit_components_article_kit_guid",
                table: "article_kit_components",
                column: "article_kit_guid");

            migrationBuilder.CreateIndex(
                name: "ix_article_packs_from_article_guid",
                table: "article_packs",
                column: "from_article_guid");

            migrationBuilder.CreateIndex(
                name: "ix_article_partition_partitions_guid",
                table: "article_partition",
                column: "partitions_guid");

            migrationBuilder.CreateIndex(
                name: "ix_article_variations_from_article_guid",
                table: "article_variations",
                column: "from_article_guid");

            migrationBuilder.CreateIndex(
                name: "ix_articles_article_guid",
                table: "articles",
                column: "article_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_articles_code128",
                table: "articles",
                column: "code128",
                unique: true,
                filter: "code128 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_code39",
                table: "articles",
                column: "code39",
                unique: true,
                filter: "code39 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_data_matrix",
                table: "articles",
                column: "data_matrix",
                unique: true,
                filter: "data_matrix IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_ean13",
                table: "articles",
                column: "ean13",
                unique: true,
                filter: "ean13 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_ean8",
                table: "articles",
                column: "ean8",
                unique: true,
                filter: "ean8 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_gs1128",
                table: "articles",
                column: "gs1128",
                unique: true,
                filter: "gs1128 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_itf14",
                table: "articles",
                column: "itf14",
                unique: true,
                filter: "itf14 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_qr_code",
                table: "articles",
                column: "qr_code",
                unique: true,
                filter: "qr_code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_upc_a",
                table: "articles",
                column: "upc_a",
                unique: true,
                filter: "upc_a IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_upc_e",
                table: "articles",
                column: "upc_e",
                unique: true,
                filter: "upc_e IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_item_partition_partitions_guid",
                table: "item_partition",
                column: "partitions_guid");

            migrationBuilder.CreateIndex(
                name: "ix_items_article_guid",
                table: "items",
                column: "article_guid");

            migrationBuilder.CreateIndex(
                name: "ix_items_article_guid1",
                table: "items",
                column: "article_guid1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_items_parent_container_guid",
                table: "items",
                column: "parent_container_guid");

            migrationBuilder.CreateIndex(
                name: "ix_partition_user_user_guid",
                table: "partition_user",
                column: "user_guid");

            migrationBuilder.CreateIndex(
                name: "ix_partition_user_group_user_group_guid",
                table: "partition_user_group",
                column: "user_group_guid");

            migrationBuilder.CreateIndex(
                name: "ix_partition_user1_user1guid",
                table: "partition_user1",
                column: "user1guid");

            migrationBuilder.CreateIndex(
                name: "ix_partitions_parent_partition_guid",
                table: "partitions",
                column: "parent_partition_guid");

            migrationBuilder.CreateIndex(
                name: "ix_partitions_user_group_guid",
                table: "partitions",
                column: "user_group_guid");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_guid",
                table: "refresh_tokens",
                column: "user_guid");

            migrationBuilder.CreateIndex(
                name: "ix_user_groups_nameid",
                table: "user_groups",
                column: "nameid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_user_group_user_guid",
                table: "user_user_group",
                column: "user_guid");

            migrationBuilder.CreateIndex(
                name: "ix_users_nameid",
                table: "users",
                column: "nameid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_article_kit_components_articles_article_guid",
                table: "article_kit_components",
                column: "article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_article_packs_articles_from_article_guid",
                table: "article_packs",
                column: "from_article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_article_partition_articles_article_guid",
                table: "article_partition",
                column: "article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_article_variations_articles_from_article_guid",
                table: "article_variations",
                column: "from_article_guid",
                principalTable: "articles",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_articles_article_kits_article_guid",
                table: "articles");

            migrationBuilder.DropForeignKey(
                name: "fk_article_packs_articles_from_article_guid",
                table: "article_packs");

            migrationBuilder.DropForeignKey(
                name: "fk_article_variations_articles_from_article_guid",
                table: "article_variations");

            migrationBuilder.DropTable(
                name: "article_kit_components");

            migrationBuilder.DropTable(
                name: "article_partition");

            migrationBuilder.DropTable(
                name: "item_partition");

            migrationBuilder.DropTable(
                name: "partition_user");

            migrationBuilder.DropTable(
                name: "partition_user_group");

            migrationBuilder.DropTable(
                name: "partition_user1");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "user_group_permissions");

            migrationBuilder.DropTable(
                name: "user_user_group");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "partitions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "item_containers");

            migrationBuilder.DropTable(
                name: "user_groups");

            migrationBuilder.DropTable(
                name: "article_kits");

            migrationBuilder.DropTable(
                name: "articles");

            migrationBuilder.DropTable(
                name: "article_containers");

            migrationBuilder.DropTable(
                name: "article_packs");

            migrationBuilder.DropTable(
                name: "article_variations");
        }
    }
}
