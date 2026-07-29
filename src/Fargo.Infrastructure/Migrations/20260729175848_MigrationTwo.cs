using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrationTwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "article_containers",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxMass = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_containers", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "article_kits",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_kits", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "item_containers",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_containers", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "user_groups",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    Nameid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_groups", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    Nameid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    DefaultPasswordExpirationPeriod = table.Column<long>(type: "bigint", nullable: true),
                    RequirePasswordChangeAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AuthVersion = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "partitions",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ParentPartitionGuid = table.Column<Guid>(type: "uuid", nullable: true),
                    UserGroupGuid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partitions", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_partitions_partitions_ParentPartitionGuid",
                        column: x => x.ParentPartitionGuid,
                        principalTable: "partitions",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_partitions_user_groups_UserGroupGuid",
                        column: x => x.UserGroupGuid,
                        principalTable: "user_groups",
                        principalColumn: "Guid");
                });

            migrationBuilder.CreateTable(
                name: "user_group_permissions",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    UserGroupGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_group_permissions", x => x.Guid);
                    table.UniqueConstraint("AK_user_group_permissions_UserGroupGuid_Action", x => new { x.UserGroupGuid, x.Action });
                    table.ForeignKey(
                        name: "FK_user_group_permissions_user_groups_UserGroupGuid",
                        column: x => x.UserGroupGuid,
                        principalTable: "user_groups",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    UserGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_users_UserGuid",
                        column: x => x.UserGuid,
                        principalTable: "users",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserUserGroup",
                columns: table => new
                {
                    UserGroupsGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    UserGuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserUserGroup", x => new { x.UserGroupsGuid, x.UserGuid });
                    table.ForeignKey(
                        name: "FK_UserUserGroup_user_groups_UserGroupsGuid",
                        column: x => x.UserGroupsGuid,
                        principalTable: "user_groups",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserUserGroup_users_UserGuid",
                        column: x => x.UserGuid,
                        principalTable: "users",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartitionUser",
                columns: table => new
                {
                    PartitionsGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    UserGuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartitionUser", x => new { x.PartitionsGuid, x.UserGuid });
                    table.ForeignKey(
                        name: "FK_PartitionUser_partitions_PartitionsGuid",
                        column: x => x.PartitionsGuid,
                        principalTable: "partitions",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartitionUser_users_UserGuid",
                        column: x => x.UserGuid,
                        principalTable: "users",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartitionUser1",
                columns: table => new
                {
                    PartitionAccessesGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    User1Guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartitionUser1", x => new { x.PartitionAccessesGuid, x.User1Guid });
                    table.ForeignKey(
                        name: "FK_PartitionUser1_partitions_PartitionAccessesGuid",
                        column: x => x.PartitionAccessesGuid,
                        principalTable: "partitions",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartitionUser1_users_User1Guid",
                        column: x => x.User1Guid,
                        principalTable: "users",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartitionUserGroup",
                columns: table => new
                {
                    PartitionsGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    UserGroupGuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartitionUserGroup", x => new { x.PartitionsGuid, x.UserGroupGuid });
                    table.ForeignKey(
                        name: "FK_PartitionUserGroup_partitions_PartitionsGuid",
                        column: x => x.PartitionsGuid,
                        principalTable: "partitions",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartitionUserGroup_user_groups_UserGroupGuid",
                        column: x => x.UserGroupGuid,
                        principalTable: "user_groups",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "article_kit_components",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    ArticleGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<double>(type: "double precision", nullable: false),
                    ArticleKitGuid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_kit_components", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_article_kit_components_article_kits_ArticleKitGuid",
                        column: x => x.ArticleKitGuid,
                        principalTable: "article_kits",
                        principalColumn: "Guid");
                });

            migrationBuilder.CreateTable(
                name: "article_packs",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    FromArticleGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_packs", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "article_variations",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    FromArticleGuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_variations", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "articles",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ArticleType = table.Column<int>(type: "integer", nullable: false),
                    ShelfLife = table.Column<long>(type: "bigint", nullable: true),
                    Color = table.Column<int>(type: "integer", nullable: true),
                    LengthX = table.Column<string>(type: "text", nullable: true),
                    LengthY = table.Column<string>(type: "text", nullable: true),
                    LengthZ = table.Column<string>(type: "text", nullable: true),
                    Mass = table.Column<string>(type: "text", nullable: true),
                    Ean13 = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    Ean8 = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    UpcA = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    UpcE = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    Code128 = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Code39 = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Itf14 = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    Gs1128 = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    QrCode = table.Column<string>(type: "character varying(2953)", maxLength: 2953, nullable: true),
                    DataMatrix = table.Column<string>(type: "character varying(2335)", maxLength: 2335, nullable: true),
                    article_guid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_articles", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_articles_article_containers_article_guid",
                        column: x => x.article_guid,
                        principalTable: "article_containers",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_articles_article_kits_article_guid",
                        column: x => x.article_guid,
                        principalTable: "article_kits",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_articles_article_packs_article_guid",
                        column: x => x.article_guid,
                        principalTable: "article_packs",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_articles_article_variations_article_guid",
                        column: x => x.article_guid,
                        principalTable: "article_variations",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArticlePartition",
                columns: table => new
                {
                    ArticleGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    PartitionsGuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticlePartition", x => new { x.ArticleGuid, x.PartitionsGuid });
                    table.ForeignKey(
                        name: "FK_ArticlePartition_articles_ArticleGuid",
                        column: x => x.ArticleGuid,
                        principalTable: "articles",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticlePartition_partitions_PartitionsGuid",
                        column: x => x.PartitionsGuid,
                        principalTable: "partitions",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    ArticleGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ParentContainerGuid = table.Column<Guid>(type: "uuid", nullable: true),
                    article_guid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_items_articles_ArticleGuid",
                        column: x => x.ArticleGuid,
                        principalTable: "articles",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_items_item_containers_article_guid",
                        column: x => x.article_guid,
                        principalTable: "item_containers",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_items_items_ParentContainerGuid",
                        column: x => x.ParentContainerGuid,
                        principalTable: "items",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ItemPartition",
                columns: table => new
                {
                    ItemGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    PartitionsGuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemPartition", x => new { x.ItemGuid, x.PartitionsGuid });
                    table.ForeignKey(
                        name: "FK_ItemPartition_items_ItemGuid",
                        column: x => x.ItemGuid,
                        principalTable: "items",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemPartition_partitions_PartitionsGuid",
                        column: x => x.PartitionsGuid,
                        principalTable: "partitions",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_article_kit_components_ArticleGuid",
                table: "article_kit_components",
                column: "ArticleGuid");

            migrationBuilder.CreateIndex(
                name: "IX_article_kit_components_ArticleKitGuid",
                table: "article_kit_components",
                column: "ArticleKitGuid");

            migrationBuilder.CreateIndex(
                name: "IX_article_packs_FromArticleGuid",
                table: "article_packs",
                column: "FromArticleGuid");

            migrationBuilder.CreateIndex(
                name: "IX_article_variations_FromArticleGuid",
                table: "article_variations",
                column: "FromArticleGuid");

            migrationBuilder.CreateIndex(
                name: "IX_ArticlePartition_PartitionsGuid",
                table: "ArticlePartition",
                column: "PartitionsGuid");

            migrationBuilder.CreateIndex(
                name: "IX_articles_article_guid",
                table: "articles",
                column: "article_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_articles_Code128",
                table: "articles",
                column: "Code128",
                unique: true,
                filter: "code128 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_articles_Code39",
                table: "articles",
                column: "Code39",
                unique: true,
                filter: "code39 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_articles_DataMatrix",
                table: "articles",
                column: "DataMatrix",
                unique: true,
                filter: "data_matrix IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_articles_Ean13",
                table: "articles",
                column: "Ean13",
                unique: true,
                filter: "ean13 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_articles_Ean8",
                table: "articles",
                column: "Ean8",
                unique: true,
                filter: "ean8 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_articles_Gs1128",
                table: "articles",
                column: "Gs1128",
                unique: true,
                filter: "gs1128 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_articles_Itf14",
                table: "articles",
                column: "Itf14",
                unique: true,
                filter: "itf14 IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_articles_QrCode",
                table: "articles",
                column: "QrCode",
                unique: true,
                filter: "qr_code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_articles_UpcA",
                table: "articles",
                column: "UpcA",
                unique: true,
                filter: "upc_a IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_articles_UpcE",
                table: "articles",
                column: "UpcE",
                unique: true,
                filter: "upc_e IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPartition_PartitionsGuid",
                table: "ItemPartition",
                column: "PartitionsGuid");

            migrationBuilder.CreateIndex(
                name: "IX_items_article_guid",
                table: "items",
                column: "article_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_items_ArticleGuid",
                table: "items",
                column: "ArticleGuid");

            migrationBuilder.CreateIndex(
                name: "IX_items_ParentContainerGuid",
                table: "items",
                column: "ParentContainerGuid");

            migrationBuilder.CreateIndex(
                name: "IX_partitions_ParentPartitionGuid",
                table: "partitions",
                column: "ParentPartitionGuid");

            migrationBuilder.CreateIndex(
                name: "IX_partitions_UserGroupGuid",
                table: "partitions",
                column: "UserGroupGuid");

            migrationBuilder.CreateIndex(
                name: "IX_PartitionUser_UserGuid",
                table: "PartitionUser",
                column: "UserGuid");

            migrationBuilder.CreateIndex(
                name: "IX_PartitionUser1_User1Guid",
                table: "PartitionUser1",
                column: "User1Guid");

            migrationBuilder.CreateIndex(
                name: "IX_PartitionUserGroup_UserGroupGuid",
                table: "PartitionUserGroup",
                column: "UserGroupGuid");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserGuid",
                table: "RefreshTokens",
                column: "UserGuid");

            migrationBuilder.CreateIndex(
                name: "IX_user_groups_Nameid",
                table: "user_groups",
                column: "Nameid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Nameid",
                table: "users",
                column: "Nameid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserUserGroup_UserGuid",
                table: "UserUserGroup",
                column: "UserGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_article_kit_components_articles_ArticleGuid",
                table: "article_kit_components",
                column: "ArticleGuid",
                principalTable: "articles",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_article_packs_articles_FromArticleGuid",
                table: "article_packs",
                column: "FromArticleGuid",
                principalTable: "articles",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_article_variations_articles_FromArticleGuid",
                table: "article_variations",
                column: "FromArticleGuid",
                principalTable: "articles",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_articles_article_kits_article_guid",
                table: "articles");

            migrationBuilder.DropForeignKey(
                name: "FK_article_packs_articles_FromArticleGuid",
                table: "article_packs");

            migrationBuilder.DropForeignKey(
                name: "FK_article_variations_articles_FromArticleGuid",
                table: "article_variations");

            migrationBuilder.DropTable(
                name: "article_kit_components");

            migrationBuilder.DropTable(
                name: "ArticlePartition");

            migrationBuilder.DropTable(
                name: "ItemPartition");

            migrationBuilder.DropTable(
                name: "PartitionUser");

            migrationBuilder.DropTable(
                name: "PartitionUser1");

            migrationBuilder.DropTable(
                name: "PartitionUserGroup");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "user_group_permissions");

            migrationBuilder.DropTable(
                name: "UserUserGroup");

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
