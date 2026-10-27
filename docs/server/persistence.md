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
`Fargo.Infrastructure/Configurations` (one per entity), keeping mapping out of
the domain types.

### Event-tracking tables

Event-tracked articles record their stock as movement events rather than
individual item rows (see [Articles and Items](../concepts/articles-and-items.md)):

- **`item_events`** — one row per event, with the article it concerns, the event
  type, and an `occured_at` timestamp. Indexed by `article_guid`, by `occured_at`,
  and by both together to support point-in-time inventory.
- **`item_moviment_events`** — the movement payload, keyed one-to-one by the
  event GUID. It stores the moved `quantity` (a `Scalar` persisted as
  `double precision`) and the optional origin/destination container references
  (`from_item_container_guid` / `to_item_container_guid`, each with a
  `*_no_container` flag preserving whether the movement originally had no
  container on that side, since the FK is set to `NULL` if the container is
  deleted).

## Repositories

Each resource has two repository interfaces in the application layer:

- a **write** repository (`IXRepository`) for loading and persisting aggregates,
- a **query** repository (`IXQueryRepository`) for read-only projections to DTOs.

Both are implemented by a single class per resource (for example
`ArticleRepository`) and registered as scoped services in
`AddFargoRepositories`. Writes are committed through `IUnitOfWork`
(`UnitOfWork`), which wraps `SaveChanges` so a handler commits all of its changes
in one transaction.

Read queries apply the actor's partition access as a filter and project directly
to DTOs, so reads never over-fetch or leak entities the actor cannot see.

Inventory for event-tracked articles is computed with raw recursive SQL
(`item_moviment_events` joined to `item_events`) as the **net of signed
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
  --project src/server/Fargo.Infrastructure \
  --startup-project src/server/Fargo.Infrastructure
```

Column and table names follow the snake_case convention automatically. Review
the generated migration before committing it.

### Applying migrations

Migrations are applied automatically at startup by the **migration service**
(`Fargo.ServiceMigration`), which calls `Database.MigrateAsync()` through an EF
execution strategy (so transient failures are retried). In the Aspire
orchestration the migration service runs before the API, and the API waits for
it to complete. See [Operations](operations.md).

## Seeding

After migrations, the **seed service** (`Fargo.ServiceSeed`) initializes the
system on first run by executing `InitializeSystemCommand`, which creates:

1. the **global partition** (the root of the partition hierarchy),
2. the **administrators** user group, and
3. the **default administrator** user.

The names, description, and default admin credentials come from configuration
(`GlobalPartition`, `Administrators`, and `DefaultAdmin` sections). See
[Operations](operations.md#configuration).
