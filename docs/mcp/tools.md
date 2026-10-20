# MCP Tools Reference

This page documents every tool the Fargo MCP server exposes. Tools are named
`fargo_<resource>_<action>`. All tools authenticate automatically before calling
the Fargo API (see [Getting Started](getting-started.md)).

Common parameters:

- `page` — page number to retrieve, starting at 1.
- `limit` — maximum number of results per page.
- `partitions` — array of partition GUIDs; results are limited to entities under
  any of them.

Tools return compact JSON projections of the underlying API DTOs, containing
only the fields an agent typically needs.

## Articles

### `fargo_articles_list`

List articles.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `page` | integer | No | Page number (starts at 1). |
| `limit` | integer | No | Max articles per page. |
| `partitions` | GUID[] | No | Only articles under any of these partitions. |

Returns an array of `{ guid, name, description, articleType, partitions }`.

### `fargo_articles_get_by_barcode`

Get a single article by one of its barcodes.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `barcode` | string | Yes | Barcode as `{value}:{type}`, e.g. `1234567890128:ean13`. |

Returns a single article summary, or null if not found.

### `fargo_articles_create`

Create a new article.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | string | Yes | The article name. |
| `description` | string | No | Optional description. |
| `articleType` | integer | No | Numeric article type. |

Returns the GUID of the created article.

### `fargo_articles_delete_by_barcode`

Delete an article identified by one of its barcodes.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `barcode` | string | Yes | Barcode as `{value}:{type}`. |

Returns a confirmation message.

## Items

### `fargo_items_list`

List items.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `page` | integer | No | Page number (starts at 1). |
| `limit` | integer | No | Max items per page. |
| `partitions` | GUID[] | No | Only items under any of these partitions. |

Returns an array of `{ guid, articleGuid, parentContainerGuid, isFixed, partitions }`.

### `fargo_items_get`

Get a single item by its GUID.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `itemGuid` | GUID | Yes | The item GUID. |

Returns a single item summary, or null if not found.

### `fargo_items_create`

Create a new item for an article.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `articleGuid` | GUID | Yes | The article this item instantiates. |
| `parentItemContainerGuid` | GUID | No | Parent container item, if any. |
| `partitionsToAdd` | GUID[] | No | Partitions to add the item to. |

Returns the GUID of the created item.

### `fargo_items_delete`

Delete an item by its GUID.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `itemGuid` | GUID | Yes | The item to delete. |

Returns a confirmation message.

## Partitions

### `fargo_partitions_list`

List partitions.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `page` | integer | No | Page number (starts at 1). |
| `limit` | integer | No | Max partitions per page. |
| `partitions` | GUID[] | No | Only partitions that are children of any of these. |

Returns an array of `{ guid, name, description, isGlobalPartition, parentPartitionGuid }`.

### `fargo_partitions_get`

Get a single partition by its GUID.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `partitionGuid` | GUID | Yes | The partition GUID. |

Returns a single partition summary, or null if not found.

### `fargo_partitions_create`

Create a new partition.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | string | Yes | The partition name. |
| `description` | string | No | Optional description. |
| `parentPartitionGuid` | GUID | No | Parent partition, if nested. |

Returns the GUID of the created partition.

### `fargo_partitions_delete`

Delete a partition by its GUID.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `partitionGuid` | GUID | Yes | The partition to delete. |

Returns a confirmation message.

## Users (read-only)

### `fargo_users_list`

List users.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `page` | integer | No | Page number (starts at 1). |
| `limit` | integer | No | Max users per page. |
| `partitions` | GUID[] | No | Only users under any of these partitions. |

Returns an array of `{ guid, nameid, isActive, isAdmin }`.

### `fargo_users_get`

Get a single user by its GUID.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `userGuid` | GUID | Yes | The user GUID. |

Returns a single user summary, or null if not found.

## User groups (read-only)

### `fargo_usergroups_list`

List user groups.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `page` | integer | No | Page number (starts at 1). |
| `limit` | integer | No | Max user groups per page. |
| `partitions` | GUID[] | No | Only user groups under any of these partitions. |

Returns an array of `{ guid, nameid, description, isActive, isAdminUserGroup, parentPartition }`.

### `fargo_usergroups_get`

Get a single user group by its GUID.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `userGroupGuid` | GUID | Yes | The user group GUID. |

Returns a single user group summary, or null if not found.

## Audit logs (read-only)

### `fargo_auditlogs_list`

List audit log entries, with optional filters.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `page` | integer | No | Page number (starts at 1). |
| `limit` | integer | No | Max entries per page. |
| `actorGuid` | GUID | No | Only entries for this actor. |
| `actorType` | integer | No | Only entries for this numeric actor type. |
| `entityGuid` | GUID | No | Only entries affecting this entity. |
| `entityType` | integer | No | Only entries affecting this numeric entity type. |
| `periodStart` | timestamp | No | Only entries at or after this UTC time (ISO 8601). |
| `periodEnd` | timestamp | No | Only entries at or before this UTC time (ISO 8601). |

Returns an array of `{ guid, actorGuid, actorType, entityGuid, entityType, auditLogType, occurredAt }`.
