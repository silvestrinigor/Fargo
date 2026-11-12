using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "authentication_attempt",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                actor_identifier = table.Column<string>(type: "text", nullable: false),
                ip_address = table.Column<string>(type: "text", nullable: false),
                occured_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                successful = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_authentication_attempt", x => x.guid);
            });

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

        migrationBuilder.CreateTable(
            name: "events",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                event_type = table.Column<int>(type: "integer", nullable: false),
                occured_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_events", x => x.guid);
            });

        migrationBuilder.CreateTable(
            name: "outbox_messages",
            columns: table => new
            {
                sequence = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                event_id = table.Column<Guid>(type: "uuid", nullable: false),
                event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                schema_version = table.Column<int>(type: "integer", nullable: false),
                aggregate_guid = table.Column<Guid>(type: "uuid", nullable: false),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                body = table.Column<string>(type: "text", nullable: false),
                processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                attempts = table.Column<int>(type: "integer", nullable: false),
                last_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                last_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_outbox_messages", x => x.sequence);
            });

        migrationBuilder.CreateTable(
            name: "actors",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                actor_type = table.Column<byte>(type: "smallint", nullable: false),
                nameid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                permissions = table.Column<string>(type: "jsonb", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_actors", x => x.guid);
                table.ForeignKey(
                    name: "fk_actors_entities_guid",
                    column: x => x.guid,
                    principalTable: "entities",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "articles",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                article_type = table.Column<byte>(type: "smallint", nullable: false),
                tracking_mode = table.Column<byte>(type: "smallint", nullable: false),
                shelf_life = table.Column<long>(type: "bigint", nullable: true),
                color = table.Column<int>(type: "integer", nullable: true),
                mass = table.Column<string>(type: "jsonb", nullable: true),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_articles", x => x.guid);
                table.ForeignKey(
                    name: "fk_articles_entities_guid",
                    column: x => x.guid,
                    principalTable: "entities",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "partitions",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                parent_partition_guid = table.Column<Guid>(type: "uuid", nullable: true),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_partitions", x => x.guid);
                table.ForeignKey(
                    name: "fk_partitions_entities_guid",
                    column: x => x.guid,
                    principalTable: "entities",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_partitions_partitions_parent_partition_guid",
                    column: x => x.parent_partition_guid,
                    principalTable: "partitions",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "user_groups",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                nameid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                parent_user_group_guid = table.Column<Guid>(type: "uuid", nullable: true),
                permissions = table.Column<string>(type: "jsonb", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_groups", x => x.guid);
                table.ForeignKey(
                    name: "fk_user_groups_entities_guid",
                    column: x => x.guid,
                    principalTable: "entities",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_user_groups_user_groups_parent_user_group_guid",
                    column: x => x.parent_user_group_guid,
                    principalTable: "user_groups",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "audit_logs",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                actor_guid = table.Column<Guid>(type: "uuid", nullable: false),
                actor_type = table.Column<byte>(type: "smallint", nullable: false),
                audit_log_type = table.Column<int>(type: "integer", nullable: false),
                command_name = table.Column<string>(type: "text", nullable: false),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                metadata = table.Column<string>(type: "jsonb", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_audit_logs", x => x.guid);
                table.ForeignKey(
                    name: "fk_audit_logs_actors_actor_guid",
                    column: x => x.actor_guid,
                    principalTable: "actors",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "service_accounts",
            columns: table => new
            {
                actor_guid = table.Column<Guid>(type: "uuid", nullable: false),
                secret_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                auth_version = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_service_accounts", x => x.actor_guid);
                table.ForeignKey(
                    name: "fk_service_accounts_actors_actor_guid",
                    column: x => x.actor_guid,
                    principalTable: "actors",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                actor_guid = table.Column<Guid>(type: "uuid", nullable: false),
                first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.actor_guid);
                table.ForeignKey(
                    name: "fk_users_actors_actor_guid",
                    column: x => x.actor_guid,
                    principalTable: "actors",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "article_barcodes",
            columns: table => new
            {
                article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                ean13 = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_article_barcodes", x => x.article_guid);
                table.ForeignKey(
                    name: "fk_article_barcodes_articles_article_guid",
                    column: x => x.article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "article_containers",
            columns: table => new
            {
                article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                max_mass = table.Column<string>(type: "jsonb", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_article_containers", x => x.article_guid);
                table.ForeignKey(
                    name: "fk_article_containers_articles_article_guid",
                    column: x => x.article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "article_dimensions",
            columns: table => new
            {
                article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                x = table.Column<string>(type: "jsonb", nullable: true),
                y = table.Column<string>(type: "jsonb", nullable: true),
                z = table.Column<string>(type: "jsonb", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_article_dimensions", x => x.article_guid);
                table.ForeignKey(
                    name: "fk_article_dimensions_articles_article_guid",
                    column: x => x.article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "article_kit_components",
            columns: table => new
            {
                kit_article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                from_article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<double>(type: "double precision", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_article_kit_components", x => x.kit_article_guid);
                table.ForeignKey(
                    name: "fk_article_kit_components_articles_from_article_guid",
                    column: x => x.from_article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_article_kit_components_articles_kit_article_guid",
                    column: x => x.kit_article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "article_packs",
            columns: table => new
            {
                pack_article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                from_article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<double>(type: "double precision", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_article_packs", x => x.pack_article_guid);
                table.ForeignKey(
                    name: "fk_article_packs_articles_from_article_guid",
                    column: x => x.from_article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_article_packs_articles_pack_article_guid",
                    column: x => x.pack_article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "article_variations",
            columns: table => new
            {
                variation_article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                from_article_guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_article_variations", x => x.variation_article_guid);
                table.ForeignKey(
                    name: "fk_article_variations_articles_from_article_guid",
                    column: x => x.from_article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_article_variations_articles_variation_article_guid",
                    column: x => x.variation_article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "item_events",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                item_event_type = table.Column<short>(type: "smallint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_item_events", x => x.guid);
                table.ForeignKey(
                    name: "fk_item_events_articles_article_guid",
                    column: x => x.article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_item_events_events_guid",
                    column: x => x.guid,
                    principalTable: "events",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "items",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                parent_item_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                is_fixed = table.Column<bool>(type: "boolean", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_items", x => x.guid);
                table.ForeignKey(
                    name: "fk_items_articles_article_guid",
                    column: x => x.article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_items_entities_guid",
                    column: x => x.guid,
                    principalTable: "entities",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_items_items_parent_item_container_guid",
                    column: x => x.parent_item_container_guid,
                    principalTable: "items",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
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

        migrationBuilder.CreateTable(
            name: "article_partitions",
            columns: table => new
            {
                article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                partition_guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_article_partitions", x => new { x.article_guid, x.partition_guid });
                table.ForeignKey(
                    name: "fk_article_partitions_articles_article_guid",
                    column: x => x.article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_article_partitions_partitions_partition_guid",
                    column: x => x.partition_guid,
                    principalTable: "partitions",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_group_partition_accesses",
            columns: table => new
            {
                user_group_guid = table.Column<Guid>(type: "uuid", nullable: false),
                partition_guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_group_partition_accesses", x => new { x.user_group_guid, x.partition_guid });
                table.ForeignKey(
                    name: "fk_user_group_partition_accesses_partitions_partition_guid",
                    column: x => x.partition_guid,
                    principalTable: "partitions",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_user_group_partition_accesses_user_groups_user_group_guid",
                    column: x => x.user_group_guid,
                    principalTable: "user_groups",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_group_partitions",
            columns: table => new
            {
                user_group_guid = table.Column<Guid>(type: "uuid", nullable: false),
                partition_guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_group_partitions", x => new { x.user_group_guid, x.partition_guid });
                table.ForeignKey(
                    name: "fk_user_group_partitions_partitions_partition_guid",
                    column: x => x.partition_guid,
                    principalTable: "partitions",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_user_group_partitions_user_groups_user_group_guid",
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
                revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_refresh_tokens", x => x.guid);
                table.ForeignKey(
                    name: "fk_refresh_tokens_users_user_guid",
                    column: x => x.user_guid,
                    principalTable: "users",
                    principalColumn: "actor_guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_authentications",
            columns: table => new
            {
                user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                default_password_expiration_period = table.Column<long>(type: "bigint", nullable: true),
                require_password_change_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                auth_version = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_authentications", x => x.user_guid);
                table.ForeignKey(
                    name: "fk_user_authentications_users_user_guid",
                    column: x => x.user_guid,
                    principalTable: "users",
                    principalColumn: "actor_guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_user_groups",
            columns: table => new
            {
                user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                user_group_guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_user_groups", x => new { x.user_guid, x.user_group_guid });
                table.ForeignKey(
                    name: "fk_user_user_groups_user_groups_user_group_guid",
                    column: x => x.user_group_guid,
                    principalTable: "user_groups",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_user_user_groups_users_user_guid",
                    column: x => x.user_guid,
                    principalTable: "users",
                    principalColumn: "actor_guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "article_container_allowed_articles",
            columns: table => new
            {
                container_article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                allowed_article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                max_quantity = table.Column<double>(type: "double precision", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_article_container_allowed_articles", x => new { x.container_article_guid, x.allowed_article_guid });
                table.ForeignKey(
                    name: "fk_article_container_allowed_articles_article_containers_conta",
                    column: x => x.container_article_guid,
                    principalTable: "article_containers",
                    principalColumn: "article_guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_article_container_allowed_articles_articles_allowed_article",
                    column: x => x.allowed_article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "item_lifecycle_events",
            columns: table => new
            {
                item_event_guid = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<double>(type: "double precision", nullable: false),
                item_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                no_container = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_item_lifecycle_events", x => x.item_event_guid);
                table.ForeignKey(
                    name: "fk_item_lifecycle_events_item_events_item_event_guid",
                    column: x => x.item_event_guid,
                    principalTable: "item_events",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_item_lifecycle_events_items_item_container_guid",
                    column: x => x.item_container_guid,
                    principalTable: "items",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "item_moviment_events",
            columns: table => new
            {
                item_event_guid = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<double>(type: "double precision", nullable: false),
                from_item_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                from_no_container = table.Column<bool>(type: "boolean", nullable: false),
                to_item_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                to_no_container = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_item_moviment_events", x => x.item_event_guid);
                table.ForeignKey(
                    name: "fk_item_moviment_events_item_events_item_event_guid",
                    column: x => x.item_event_guid,
                    principalTable: "item_events",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_item_moviment_events_items_from_item_container_guid",
                    column: x => x.from_item_container_guid,
                    principalTable: "items",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_item_moviment_events_items_to_item_container_guid",
                    column: x => x.to_item_container_guid,
                    principalTable: "items",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "item_moviments",
            columns: table => new
            {
                item_guid = table.Column<Guid>(type: "uuid", nullable: false),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                moved_to_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                removed_from_containers = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_item_moviments", x => new { x.item_guid, x.occurred_at });
                table.ForeignKey(
                    name: "fk_item_moviments_items_item_guid",
                    column: x => x.item_guid,
                    principalTable: "items",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_item_moviments_items_moved_to_container_guid",
                    column: x => x.moved_to_container_guid,
                    principalTable: "items",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "item_parent_container_history",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                valid_period = table.Column<NpgsqlRange<DateTime>>(type: "tstzrange", nullable: false, defaultValueSql: "tstzrange(now(), 'infinity', '[)')"),
                item_guid = table.Column<Guid>(type: "uuid", nullable: false),
                parent_item_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                removed_from_containers = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_item_parent_container_history", x => x.id);
                table.ForeignKey(
                    name: "fk_item_parent_container_history_items_item_guid",
                    column: x => x.item_guid,
                    principalTable: "items",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_item_parent_container_history_items_parent_item_container_g",
                    column: x => x.parent_item_container_guid,
                    principalTable: "items",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "item_partitions",
            columns: table => new
            {
                item_guid = table.Column<Guid>(type: "uuid", nullable: false),
                partition_guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_item_partitions", x => new { x.item_guid, x.partition_guid });
                table.ForeignKey(
                    name: "fk_item_partitions_items_item_guid",
                    column: x => x.item_guid,
                    principalTable: "items",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_item_partitions_partitions_partition_guid",
                    column: x => x.partition_guid,
                    principalTable: "partitions",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "supply_orders",
            columns: table => new
            {
                guid = table.Column<Guid>(type: "uuid", nullable: false),
                reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                status = table.Column<byte>(type: "smallint", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                placed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                receiving_container_guid = table.Column<Guid>(type: "uuid", nullable: true),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_supply_orders", x => x.guid);
                table.ForeignKey(
                    name: "fk_supply_orders_entities_guid",
                    column: x => x.guid,
                    principalTable: "entities",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_supply_orders_items_receiving_container_guid",
                    column: x => x.receiving_container_guid,
                    principalTable: "items",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "supply_order_lines",
            columns: table => new
            {
                supply_order_guid = table.Column<Guid>(type: "uuid", nullable: false),
                article_guid = table.Column<Guid>(type: "uuid", nullable: false),
                ordered_quantity = table.Column<double>(type: "double precision", nullable: false),
                received_quantity = table.Column<double>(type: "double precision", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_supply_order_lines", x => new { x.supply_order_guid, x.article_guid });
                table.ForeignKey(
                    name: "fk_supply_order_lines_articles_article_guid",
                    column: x => x.article_guid,
                    principalTable: "articles",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_supply_order_lines_supply_orders_supply_order_guid",
                    column: x => x.supply_order_guid,
                    principalTable: "supply_orders",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "supply_order_partitions",
            columns: table => new
            {
                supply_order_guid = table.Column<Guid>(type: "uuid", nullable: false),
                partition_guid = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_supply_order_partitions", x => new { x.supply_order_guid, x.partition_guid });
                table.ForeignKey(
                    name: "fk_supply_order_partitions_partitions_partition_guid",
                    column: x => x.partition_guid,
                    principalTable: "partitions",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_supply_order_partitions_supply_orders_supply_order_guid",
                    column: x => x.supply_order_guid,
                    principalTable: "supply_orders",
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

        migrationBuilder.CreateIndex(
            name: "ix_actors_nameid",
            table: "actors",
            column: "nameid",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_article_barcodes_ean13",
            table: "article_barcodes",
            column: "ean13",
            unique: true,
            filter: "ean13 IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_article_container_allowed_articles_allowed_article_guid",
            table: "article_container_allowed_articles",
            column: "allowed_article_guid");

        migrationBuilder.CreateIndex(
            name: "ix_article_kit_components_from_article_guid",
            table: "article_kit_components",
            column: "from_article_guid");

        migrationBuilder.CreateIndex(
            name: "ix_article_packs_from_article_guid",
            table: "article_packs",
            column: "from_article_guid");

        migrationBuilder.CreateIndex(
            name: "ix_article_partitions_partition_guid",
            table: "article_partitions",
            column: "partition_guid");

        migrationBuilder.CreateIndex(
            name: "ix_article_variations_from_article_guid",
            table: "article_variations",
            column: "from_article_guid");

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_actor_guid_occurred_at",
            table: "audit_logs",
            columns: new[] { "actor_guid", "occurred_at" });

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_occurred_at",
            table: "audit_logs",
            column: "occurred_at");

        migrationBuilder.CreateIndex(
            name: "ix_authentication_attempt_ip_address_actor_identifier_occured_",
            table: "authentication_attempt",
            columns: new[] { "ip_address", "actor_identifier", "occured_at", "successful" });

        migrationBuilder.CreateIndex(
            name: "ix_entities_entity_type",
            table: "entities",
            column: "entity_type");

        migrationBuilder.CreateIndex(
            name: "ix_events_event_type",
            table: "events",
            column: "event_type");

        migrationBuilder.CreateIndex(
            name: "ix_events_occured_at",
            table: "events",
            column: "occured_at");

        migrationBuilder.CreateIndex(
            name: "ix_item_events_article_guid",
            table: "item_events",
            column: "article_guid");

        migrationBuilder.CreateIndex(
            name: "ix_item_lifecycle_events_item_container_guid",
            table: "item_lifecycle_events",
            column: "item_container_guid");

        migrationBuilder.CreateIndex(
            name: "ix_item_moviment_events_from_item_container_guid",
            table: "item_moviment_events",
            column: "from_item_container_guid");

        migrationBuilder.CreateIndex(
            name: "ix_item_moviment_events_to_item_container_guid",
            table: "item_moviment_events",
            column: "to_item_container_guid");

        migrationBuilder.CreateIndex(
            name: "ix_item_moviments_moved_to_container_guid",
            table: "item_moviments",
            column: "moved_to_container_guid");

        migrationBuilder.CreateIndex(
            name: "ix_item_parent_container_history_item_guid",
            table: "item_parent_container_history",
            column: "item_guid");

        migrationBuilder.CreateIndex(
            name: "ix_item_parent_container_history_parent_item_container_guid",
            table: "item_parent_container_history",
            column: "parent_item_container_guid");

        migrationBuilder.CreateIndex(
            name: "ix_item_partitions_partition_guid",
            table: "item_partitions",
            column: "partition_guid");

        migrationBuilder.CreateIndex(
            name: "ix_items_article_guid",
            table: "items",
            column: "article_guid");

        migrationBuilder.CreateIndex(
            name: "ix_items_parent_item_container_guid",
            table: "items",
            column: "parent_item_container_guid");

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_event_id",
            table: "outbox_messages",
            column: "event_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_processed_at",
            table: "outbox_messages",
            column: "processed_at");

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_unprocessed_sequence",
            table: "outbox_messages",
            column: "sequence",
            filter: "processed_at IS NULL");

        migrationBuilder.CreateIndex(
            name: "ix_partitions_parent_partition_guid",
            table: "partitions",
            column: "parent_partition_guid");

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
            name: "ix_supply_order_lines_article_guid",
            table: "supply_order_lines",
            column: "article_guid");

        migrationBuilder.CreateIndex(
            name: "ix_supply_order_partitions_partition_guid",
            table: "supply_order_partitions",
            column: "partition_guid");

        migrationBuilder.CreateIndex(
            name: "ix_supply_orders_receiving_container_guid",
            table: "supply_orders",
            column: "receiving_container_guid");

        migrationBuilder.CreateIndex(
            name: "ix_supply_orders_status",
            table: "supply_orders",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "ix_user_group_partition_accesses_partition_guid",
            table: "user_group_partition_accesses",
            column: "partition_guid");

        migrationBuilder.CreateIndex(
            name: "ix_user_group_partitions_partition_guid",
            table: "user_group_partitions",
            column: "partition_guid");

        migrationBuilder.CreateIndex(
            name: "ix_user_groups_nameid",
            table: "user_groups",
            column: "nameid",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_user_groups_parent_user_group_guid",
            table: "user_groups",
            column: "parent_user_group_guid");

        migrationBuilder.CreateIndex(
            name: "ix_user_user_groups_user_group_guid",
            table: "user_user_groups",
            column: "user_group_guid");

        // Raw-SQL objects EF Core cannot model (btree_gist, the history
        // exclusion constraint, the trigger and the GiST lookup index).
        migrationBuilder.CreateItemParentContainerHistoryTrigger();
        migrationBuilder.CreateItemParentContainerHistoryParentPeriodIndex();
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropItemParentContainerHistoryParentPeriodIndex();
        migrationBuilder.DropItemParentContainerHistoryTrigger();
        migrationBuilder.DropTable(
            name: "actor_partition_accesses");

        migrationBuilder.DropTable(
            name: "actor_partitions");

        migrationBuilder.DropTable(
            name: "article_barcodes");

        migrationBuilder.DropTable(
            name: "article_container_allowed_articles");

        migrationBuilder.DropTable(
            name: "article_dimensions");

        migrationBuilder.DropTable(
            name: "article_kit_components");

        migrationBuilder.DropTable(
            name: "article_packs");

        migrationBuilder.DropTable(
            name: "article_partitions");

        migrationBuilder.DropTable(
            name: "article_variations");

        migrationBuilder.DropTable(
            name: "audit_logs");

        migrationBuilder.DropTable(
            name: "authentication_attempt");

        migrationBuilder.DropTable(
            name: "item_lifecycle_events");

        migrationBuilder.DropTable(
            name: "item_moviment_events");

        migrationBuilder.DropTable(
            name: "item_moviments");

        migrationBuilder.DropTable(
            name: "item_parent_container_history");

        migrationBuilder.DropTable(
            name: "item_partitions");

        migrationBuilder.DropTable(
            name: "outbox_messages");

        migrationBuilder.DropTable(
            name: "refresh_tokens");

        migrationBuilder.DropTable(
            name: "service_accounts");

        migrationBuilder.DropTable(
            name: "supply_order_lines");

        migrationBuilder.DropTable(
            name: "supply_order_partitions");

        migrationBuilder.DropTable(
            name: "user_authentications");

        migrationBuilder.DropTable(
            name: "user_group_partition_accesses");

        migrationBuilder.DropTable(
            name: "user_group_partitions");

        migrationBuilder.DropTable(
            name: "user_user_groups");

        migrationBuilder.DropTable(
            name: "article_containers");

        migrationBuilder.DropTable(
            name: "item_events");

        migrationBuilder.DropTable(
            name: "supply_orders");

        migrationBuilder.DropTable(
            name: "partitions");

        migrationBuilder.DropTable(
            name: "user_groups");

        migrationBuilder.DropTable(
            name: "users");

        migrationBuilder.DropTable(
            name: "events");

        migrationBuilder.DropTable(
            name: "items");

        migrationBuilder.DropTable(
            name: "actors");

        migrationBuilder.DropTable(
            name: "articles");

        migrationBuilder.DropTable(
            name: "entities");
    }
}
