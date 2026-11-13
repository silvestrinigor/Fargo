using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fargo.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddEnumLookupsAndNormalizePermissions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {

        migrationBuilder.CreateTable(
            name: "action_types",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_action_types", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "actor_types",
            columns: table => new
            {
                id = table.Column<byte>(type: "smallint", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_actor_types", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "article_tracking_modes",
            columns: table => new
            {
                id = table.Column<byte>(type: "smallint", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_article_tracking_modes", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "article_types",
            columns: table => new
            {
                id = table.Column<byte>(type: "smallint", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_article_types", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "audit_log_types",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_audit_log_types", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "entity_types",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_entity_types", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "event_types",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_event_types", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "item_event_types",
            columns: table => new
            {
                id = table.Column<short>(type: "smallint", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_item_event_types", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "supply_order_statuses",
            columns: table => new
            {
                id = table.Column<byte>(type: "smallint", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_supply_order_statuses", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "actor_permissions",
            columns: table => new
            {
                actor_guid = table.Column<Guid>(type: "uuid", nullable: false),
                action_type = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_actor_permissions", x => new { x.actor_guid, x.action_type });
                table.ForeignKey(
                    name: "fk_actor_permissions_action_types_action_type",
                    column: x => x.action_type,
                    principalTable: "action_types",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_actor_permissions_actors_actor_guid",
                    column: x => x.actor_guid,
                    principalTable: "actors",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_group_permissions",
            columns: table => new
            {
                user_group_guid = table.Column<Guid>(type: "uuid", nullable: false),
                action_type = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_group_permissions", x => new { x.user_group_guid, x.action_type });
                table.ForeignKey(
                    name: "fk_user_group_permissions_action_types_action_type",
                    column: x => x.action_type,
                    principalTable: "action_types",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_user_group_permissions_user_groups_user_group_guid",
                    column: x => x.user_group_guid,
                    principalTable: "user_groups",
                    principalColumn: "guid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            table: "action_types",
            columns: new[] { "id", "name" },
            values: new object[,]
            {
                { 1, "CreateArticle" },
                { 2, "DeleteArticle" },
                { 3, "EditArticle" },
                { 4, "CreateItem" },
                { 5, "DeleteItem" },
                { 6, "EditItem" },
                { 7, "CreateUser" },
                { 8, "DeleteUser" },
                { 9, "EditUser" },
                { 10, "ChangeAnotherUserPassword" },
                { 11, "CreateUserGroup" },
                { 12, "DeleteUserGroup" },
                { 13, "EditUserGroup" },
                { 14, "ChangeUserGroupMembers" },
                { 15, "CreatePartition" },
                { 16, "DeletePartition" },
                { 17, "EditPartition" },
                { 18, "ReadAuditLogs" },
                { 19, "ReadItemEvents" },
                { 20, "CreateServiceAccount" },
                { 21, "DeleteServiceAccount" },
                { 22, "EditServiceAccount" },
                { 23, "ReadServiceAccount" },
                { 24, "CreateSupplyOrder" },
                { 25, "EditSupplyOrder" },
                { 26, "PlaceSupplyOrder" },
                { 27, "ReceiveSupplyOrder" },
                { 28, "CancelSupplyOrder" },
                { 29, "CloseSupplyOrder" },
                { 30, "ReadSupplyOrders" },
                { 31, "ActivateUser" },
                { 32, "ActivateUserGroup" },
                { 33, "ActivateServiceAccount" },
                { 34, "MoveItem" },
                { 35, "ChangeUserPermissions" },
                { 36, "ChangeUserGroupPermissions" },
                { 37, "ChangeServiceAccountPermissions" },
                { 38, "ChangeUserPartitions" },
                { 39, "ChangeUserGroupPartitions" },
                { 40, "ChangeServiceAccountPartitions" },
                { 41, "ChangeUserGroupParent" },
                { 42, "ChangePartitionParent" },
                { 43, "RotateServiceAccountSecret" },
                { 44, "ChangeArticlePartitions" },
                { 45, "ChangeItemPartitions" }
            });

        migrationBuilder.InsertData(
            table: "actor_types",
            columns: new[] { "id", "name" },
            values: new object[,]
            {
                { (byte)1, "User" },
                { (byte)2, "Service" }
            });

        migrationBuilder.InsertData(
            table: "article_tracking_modes",
            columns: new[] { "id", "name" },
            values: new object[,]
            {
                { (byte)1, "Entity" },
                { (byte)2, "Event" }
            });

        migrationBuilder.InsertData(
            table: "article_types",
            columns: new[] { "id", "name" },
            values: new object[,]
            {
                { (byte)1, "Default" },
                { (byte)2, "Variation" },
                { (byte)3, "Pack" },
                { (byte)4, "Kit" },
                { (byte)5, "Container" }
            });

        migrationBuilder.InsertData(
            table: "audit_log_types",
            columns: new[] { "id", "name" },
            values: new object[,]
            {
                { 1, "EntityCreated" },
                { 2, "EntityUpdated" },
                { 3, "EntityDeleted" },
                { 4, "ItemEventRecorded" },
                { 5, "Authenticated" },
                { 6, "LoggedOut" },
                { 7, "TokenRefreshed" },
                { 8, "ServiceTokenIssued" },
                { 9, "PasswordChanged" },
                { 10, "ServiceAccountSecretRotated" },
                { 11, "SystemInitialized" },
                { 12, "SupplyOrderPlaced" },
                { 13, "SupplyOrderReceived" },
                { 14, "SupplyOrderCancelled" },
                { 15, "SupplyOrderClosed" },
                { 16, "PermissionsChanged" }
            });

        migrationBuilder.InsertData(
            table: "entity_types",
            columns: new[] { "id", "name" },
            values: new object[,]
            {
                { 0, "Article" },
                { 1, "Item" },
                { 2, "Actor" },
                { 3, "UserGroup" },
                { 4, "Partition" },
                { 5, "SupplyOrder" }
            });

        migrationBuilder.InsertData(
            table: "event_types",
            columns: new[] { "id", "name" },
            values: new object[] { 0, "Item" });

        migrationBuilder.InsertData(
            table: "item_event_types",
            columns: new[] { "id", "name" },
            values: new object[,]
            {
                { (short)1, "Moviment" },
                { (short)2, "Creation" },
                { (short)3, "Deletion" }
            });

        migrationBuilder.InsertData(
            table: "supply_order_statuses",
            columns: new[] { "id", "name" },
            values: new object[,]
            {
                { (byte)1, "Draft" },
                { (byte)2, "Placed" },
                { (byte)3, "PartiallyReceived" },
                { (byte)4, "Received" },
                { (byte)5, "Closed" },
                { (byte)6, "Cancelled" }
            });

        // Hand-edited: move the permissions out of the jsonb columns before dropping them. EF Core would
        // drop the columns first and lose every grant. An unknown number in the old data fails the
        // foreign key to action_types, which stops the migration instead of silently dropping the grant.
        migrationBuilder.Sql(
            """
            INSERT INTO actor_permissions (actor_guid, action_type)
            SELECT DISTINCT a.guid, permission.value::integer
            FROM actors AS a
            CROSS JOIN LATERAL jsonb_array_elements_text(a.permissions) AS permission(value);

            INSERT INTO user_group_permissions (user_group_guid, action_type)
            SELECT DISTINCT ug.guid, permission.value::integer
            FROM user_groups AS ug
            CROSS JOIN LATERAL jsonb_array_elements_text(ug.permissions) AS permission(value);
            """);

        migrationBuilder.DropColumn(
            name: "permissions",
            table: "user_groups");

        migrationBuilder.DropColumn(
            name: "permissions",
            table: "actors");
        migrationBuilder.CreateIndex(
            name: "ix_item_events_item_event_type",
            table: "item_events",
            column: "item_event_type");

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_actor_type",
            table: "audit_logs",
            column: "actor_type");

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_audit_log_type",
            table: "audit_logs",
            column: "audit_log_type");

        migrationBuilder.CreateIndex(
            name: "ix_articles_article_type",
            table: "articles",
            column: "article_type");

        migrationBuilder.CreateIndex(
            name: "ix_articles_tracking_mode",
            table: "articles",
            column: "tracking_mode");

        migrationBuilder.CreateIndex(
            name: "ix_actors_actor_type",
            table: "actors",
            column: "actor_type");

        migrationBuilder.CreateIndex(
            name: "ix_action_types_name",
            table: "action_types",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_actor_permissions_action_type",
            table: "actor_permissions",
            column: "action_type");

        migrationBuilder.CreateIndex(
            name: "ix_actor_types_name",
            table: "actor_types",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_article_tracking_modes_name",
            table: "article_tracking_modes",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_article_types_name",
            table: "article_types",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_audit_log_types_name",
            table: "audit_log_types",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_entity_types_name",
            table: "entity_types",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_event_types_name",
            table: "event_types",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_item_event_types_name",
            table: "item_event_types",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_supply_order_statuses_name",
            table: "supply_order_statuses",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_user_group_permissions_action_type",
            table: "user_group_permissions",
            column: "action_type");

        migrationBuilder.AddForeignKey(
            name: "fk_actors_actor_types_actor_type",
            table: "actors",
            column: "actor_type",
            principalTable: "actor_types",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "fk_articles_article_tracking_modes_tracking_mode",
            table: "articles",
            column: "tracking_mode",
            principalTable: "article_tracking_modes",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "fk_articles_article_types_article_type",
            table: "articles",
            column: "article_type",
            principalTable: "article_types",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "fk_audit_logs_actor_types_actor_type",
            table: "audit_logs",
            column: "actor_type",
            principalTable: "actor_types",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "fk_audit_logs_audit_log_types_audit_log_type",
            table: "audit_logs",
            column: "audit_log_type",
            principalTable: "audit_log_types",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "fk_entities_entity_types_entity_type",
            table: "entities",
            column: "entity_type",
            principalTable: "entity_types",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "fk_events_event_types_event_type",
            table: "events",
            column: "event_type",
            principalTable: "event_types",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "fk_item_events_item_event_types_item_event_type",
            table: "item_events",
            column: "item_event_type",
            principalTable: "item_event_types",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "fk_supply_orders_supply_order_statuses_status",
            table: "supply_orders",
            column: "status",
            principalTable: "supply_order_statuses",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Hand-edited: put the permissions back into jsonb columns before the tables are dropped.
        migrationBuilder.AddColumn<string>(
            name: "permissions",
            table: "user_groups",
            type: "jsonb",
            nullable: false,
            defaultValue: "[]");

        migrationBuilder.AddColumn<string>(
            name: "permissions",
            table: "actors",
            type: "jsonb",
            nullable: false,
            defaultValue: "[]");

        migrationBuilder.Sql(
            """
            UPDATE actors AS a
            SET permissions = granted.permissions
            FROM (
                SELECT actor_guid, jsonb_agg(action_type ORDER BY action_type) AS permissions
                FROM actor_permissions
                GROUP BY actor_guid
            ) AS granted
            WHERE granted.actor_guid = a.guid;

            UPDATE user_groups AS ug
            SET permissions = granted.permissions
            FROM (
                SELECT user_group_guid, jsonb_agg(action_type ORDER BY action_type) AS permissions
                FROM user_group_permissions
                GROUP BY user_group_guid
            ) AS granted
            WHERE granted.user_group_guid = ug.guid;
            """);

        migrationBuilder.DropForeignKey(
            name: "fk_actors_actor_types_actor_type",
            table: "actors");

        migrationBuilder.DropForeignKey(
            name: "fk_articles_article_tracking_modes_tracking_mode",
            table: "articles");

        migrationBuilder.DropForeignKey(
            name: "fk_articles_article_types_article_type",
            table: "articles");

        migrationBuilder.DropForeignKey(
            name: "fk_audit_logs_actor_types_actor_type",
            table: "audit_logs");

        migrationBuilder.DropForeignKey(
            name: "fk_audit_logs_audit_log_types_audit_log_type",
            table: "audit_logs");

        migrationBuilder.DropForeignKey(
            name: "fk_entities_entity_types_entity_type",
            table: "entities");

        migrationBuilder.DropForeignKey(
            name: "fk_events_event_types_event_type",
            table: "events");

        migrationBuilder.DropForeignKey(
            name: "fk_item_events_item_event_types_item_event_type",
            table: "item_events");

        migrationBuilder.DropForeignKey(
            name: "fk_supply_orders_supply_order_statuses_status",
            table: "supply_orders");

        migrationBuilder.DropTable(
            name: "actor_permissions");

        migrationBuilder.DropTable(
            name: "actor_types");

        migrationBuilder.DropTable(
            name: "article_tracking_modes");

        migrationBuilder.DropTable(
            name: "article_types");

        migrationBuilder.DropTable(
            name: "audit_log_types");

        migrationBuilder.DropTable(
            name: "entity_types");

        migrationBuilder.DropTable(
            name: "event_types");

        migrationBuilder.DropTable(
            name: "item_event_types");

        migrationBuilder.DropTable(
            name: "supply_order_statuses");

        migrationBuilder.DropTable(
            name: "user_group_permissions");

        migrationBuilder.DropTable(
            name: "action_types");

        migrationBuilder.DropIndex(
            name: "ix_item_events_item_event_type",
            table: "item_events");

        migrationBuilder.DropIndex(
            name: "ix_audit_logs_actor_type",
            table: "audit_logs");

        migrationBuilder.DropIndex(
            name: "ix_audit_logs_audit_log_type",
            table: "audit_logs");

        migrationBuilder.DropIndex(
            name: "ix_articles_article_type",
            table: "articles");

        migrationBuilder.DropIndex(
            name: "ix_articles_tracking_mode",
            table: "articles");

        migrationBuilder.DropIndex(
            name: "ix_actors_actor_type",
            table: "actors");

    }
}
