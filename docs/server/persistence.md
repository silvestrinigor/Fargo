# Persistence

Persistence lives in `Fargo.Infrastructure`. It implements the repository
interfaces defined by the application layer using **Entity Framework Core 10**
over **PostgreSQL** (via **Npgsql**).

## DbContext and conventions

`FargoDbContext` is the EF Core context. It is registered by
`AddFargoInfrastructure` and configured to:

- Use Npgsql with the `fargo-database` connection string.
- Apply the **snake_case naming convention** (`UseSnakeCaseNamingConvention`),
  so `ParentPartitionGuid` becomes the column `parent_partition_guid` and
  `AuditLog` becomes the table `audit_logs`.
- Record applied migrations in the `__ef_migrations_history` table.

Entity-to-table mapping is defined by `IEntityTypeConfiguration<T>` classes under
`Fargo.Infrastructure/Persistence/Configurations` (one per entity), keeping
mapping out of the domain types.

### Save-changes interceptors

Two interceptors run on every `SaveChanges`, whichever code path calls it, and
add their rows in the same transaction as the change:

| Interceptor | Writes | Purpose |
| --- | --- | --- |
| `OutboxSaveChangesInterceptor` | `outbox_messages` | An integration event for each changed article. See [Integration Events](integration-events.md). |
| `AuthorizationStampSaveChangesInterceptor` | `authorization_stamps` | A new authorization stamp when permissions or partition access change, which invalidates the authorization cache. See [Security](security.md#how-the-principal-is-read-and-cached). |

Changes made outside the change tracker (raw SQL, `ExecuteUpdate`,
`ExecuteDelete`) bypass both.

### Item events

Stock changes are stored in one table, **`item_events`** (joined to the `events`
base table that holds `occured_at`). Each row is a movement of a `quantity` (a
`Scalar` persisted as `double precision`) from an origin container to a
destination container (`from_item_container_guid` / `to_item_container_guid`),
each with a `*_no_container` flag that preserves whether the event originally had
no container on that side, since the FK is set to `NULL` if the container is
deleted. The event type says what a missing side means: a creation has no origin,
a deletion has no destination. Indexed by `article_guid`, `item_guid`, `occured_at`,
and by article and time together to support point-in-time inventory.

- Event-tracked articles have no per-unit item, so clients record their stock
  through the API and `item_guid` is `NULL`.
- Entity-tracked articles get a row, written by the application in the same
  transaction, when an item is created, moved or deleted. `item_guid` holds the
  item's GUID and the quantity is 1. It is deliberately not a foreign key, so the
  events outlive the item. Deleting a container deletes the items inside it with
  a cascading foreign key, so the delete command first records a deletion for each
  contained item. Those events keep no reference to the deleted container, only
  the `from_no_container = false` flag.
### Reference tables

Every enum stored in a column has a **reference table** with `id` and `name`,
and the column is a foreign key to it. The database can then tell what a number
means and rejects numbers that are not members of the enum.

| Table | Enum | Referenced by |
| --- | --- | --- |
| `entity_types` | `EntityType` | `entities.entity_type` |
| `event_types` | `EventType` | `events.event_type` |
| `item_event_types` | `ItemEventType` | `item_events.item_event_type` |
| `actor_types` | `ActorType` | `actors.actor_type`, `audit_logs.actor_type` |
| `article_types` | `ArticleType` | `articles.article_type` |
| `article_tracking_modes` | `ArticleTrackingMode` | `articles.tracking_mode` |
| `supply_order_statuses` | `SupplyOrderStatus` | `supply_orders.status` |
| `audit_log_types` | `AuditLogType` | `audit_logs.audit_log_type` |
| `action_types` | `ActionType` | `actor_permissions`, `user_group_permissions` |

The enum is the only source of truth. The code uses it directly and never reads
these tables. Their rows are generated from the enum by `EnumLookupConfiguration`
(EF Core `HasData`), so adding a member and then adding a migration inserts the
new row. Members are append-only: renumbering or removing one fails while rows
still use it. `EnumLookupTests` checks that each table matches its enum.

Permissions are rows, not arrays: `actor_permissions` (permissions granted
directly to an actor) and `user_group_permissions`, each keyed by the owner and
the `action_type`. Deleting the actor or group deletes its permissions.

## Repositories

Each resource has two repository interfaces in the application layer:

- a **write** repository (`IXRepository`) for loading and persisting aggregates,
- a **query** repository (`IXQueryRepository`) for read-only projections to DTOs.

Both are implemented by a single class per resource (for example
`ArticleRepository`) and registered as scoped services in
`AddFargoRepositories`. Writes are committed through `IUnitOfWork`
(`UnitOfWork`), which wraps `SaveChanges` so a handler commits all of its changes
in one transaction. Commands that must read and write consistently run inside an
explicit transaction instead (see [Architecture](architecture.md#cqrs)).

Read queries apply the actor's partition access as a filter and project directly
to DTOs, so reads never over-fetch or leak entities the actor cannot see.

Inventory for event-tracked articles is computed with raw recursive SQL
(movement rows of `item_events`, for event-tracked articles) as the **net of signed
quantities** — quantity moved into a container minus quantity moved out —
optionally bounded by an `occured_at <= at` instant and expanded over the live
container subtree. The article and container inventory query handlers branch on
the article's tracking mode and merge entity-based counts with event-based net
quantities per article.

## Audit logs

Audit logs are stored in the `audit_logs` table. The metadata bag is serialized
to a PostgreSQL **`jsonb`** column through a value converter, so arbitrary typed
metadata (strings, numbers, booleans, nested objects and arrays) is stored
without a fixed schema. Indexes support the common access patterns — by entity
and time, by actor and time, and by time alone.

## Migrations

Migrations are EF Core migrations under `Fargo.Infrastructure/Migrations`. A
design-time factory (`FargoDesignTimeDbContextFactory`) lets the EF tools build
the context without running the host.

### Adding a migration

From the repository root, after changing the model or an entity configuration:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Fargo.Infrastructure \
  --startup-project src/Fargo.Infrastructure
```

Column and table names follow the snake_case convention automatically. Review
the generated migration before committing it. EF Core cannot move data between
shapes, so when a change replaces a column (as `AddEnumLookupsAndNormalizePermissions`
did with the jsonb permissions), edit the migration to copy the data in `Up` and
`Down`, mark the edit with a comment, and test both directions.

### Applying migrations

Migrations are applied automatically at startup by the **migration service**
(`Fargo.Migrator`), which calls `Database.MigrateAsync()` through an EF
execution strategy (so transient failures are retried). In the Aspire
orchestration the migration service runs before the API, and the API waits for
it to complete. See [Operations](operations.md).

## System initialization

After migrations, the **initializer service** (`Fargo.Initializer`) initializes the
system on first run by executing `InitializeSystemCommand`, which creates:

1. the **global partition** (the root of the partition hierarchy),
2. the **administrators** user group, and
3. the **default administrator** user.

The names, description, and default admin credentials come from configuration
(`GlobalPartition`, `Administrators`, and `DefaultAdmin` sections). See
[Operations](operations.md#configuration).
