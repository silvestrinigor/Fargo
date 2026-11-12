# Fargo.SimulationWorker

A long-running **client-activity simulation worker** for the Fargo system. It
drives the real HTTP API of a running instance in an (optionally) infinite loop,
simulating a busy warehouse so the system accumulates realistic client/user
history: articles, items, containers, moves, removals, deletions, event-tracked
bulk movements, and read-side queries.

It is an internal test/driver project — not a shipped artifact.

## What it does

On startup the worker:

1. Authenticates as a bootstrap **admin**.
2. Builds a small **world**: a partition tree (`Site > Zone-1..3`), one user
   group per role (with the permissions and partition access that role needs),
   and one **operator user** per configured operator.
3. Launches one concurrent **operator loop** per operator. Each operator has a
   **role / persona** and repeatedly performs weighted actions, with a
   randomised think-time between them, until Ctrl+C (or an optional bound).
4. Prints a periodic **metrics summary** (ops/min, per-action counts, errors,
   429s) and a final summary on shutdown.

### Personas

| Role          | Flow            | Typical actions |
|---------------|-----------------|-----------------|
| `Receiver`    | entity-tracked  | create product articles, containers, items |
| `PutAway`     | entity-tracked  | create items, relocate them between containers |
| `Picker`      | entity-tracked  | move, remove, ship (delete) items |
| `BulkHandler` | event-tracked   | create event-tracked articles; record receipt / transfer / dispatch movement events |
| `Auditor`     | read-only       | list articles/items, item events, audit logs |

The number of operators (`Operators`) is distributed round-robin across the
enabled personas, so raising it adds more concurrent workers (and, because each
operator has its own session, its own per-user rate-limit bucket).

## Running

Point it at a running API and run:

```sh
dotnet run --project tests/client/Fargo.SimulationWorker
```

By default it runs **forever** until you press Ctrl+C.

### Common overrides

Any setting in the `SimulationWorker` configuration section can be overridden by
environment variable or command-line argument:

```sh
# 8 operators, point at a specific server
dotnet run --project tests/client/Fargo.SimulationWorker -- \
  --SimulationWorker:Operators=8 \
  --SimulationWorker:Server=https://localhost:7563

# bounded smoke run for CI: stop after 30 seconds
dotnet run --project tests/client/Fargo.SimulationWorker -- \
  --SimulationWorker:MaxDurationSeconds=30

# stop each operator after 100 actions
dotnet run --project tests/client/Fargo.SimulationWorker -- \
  --SimulationWorker:MaxIterationsPerOperator=100

# via environment variable
$env:SimulationWorker__Server = "https://localhost:7563"
```

## Configuration (`appsettings.json` → `SimulationWorker`)

| Key | Default | Meaning |
|-----|---------|---------|
| `Server` | `https://localhost:7563` | Base address of the Fargo API |
| `Username` / `Password` | `admin` / `Senha123@` | Bootstrap admin credentials |
| `Operators` | `5` | Number of concurrent virtual operators |
| `NewUserPassword` | `Simulate123@` | Password for created operators (must satisfy the policy) |
| `RunTag` | `""` | Unique suffix for generated names (timestamp when empty) |
| `Seed` | `20260101` | RNG seed (reproducible runs) |
| `MinThinkTimeMs` / `MaxThinkTimeMs` | `250` / `1500` | Delay range between an operator's actions |
| `MaxDurationSeconds` | `0` | Wall-clock cap (`0` = infinite) |
| `MaxIterationsPerOperator` | `0` | Per-operator action cap (`0` = infinite) |
| `MetricsIntervalSeconds` | `15` | Metrics summary interval |
| `EnableConsistencyChecks` | `true` | Occasionally read back writes and assert (logged, never fatal) |
| `EnableEntityFlow` | `true` | Enable the entity-tracked personas |
| `EnableEventFlow` | `true` | Enable the event-tracked bulk persona |
| `EnableAuditorFlow` | `true` | Enable the read-only auditor persona |
| `RateLimitBaseBackoffMs` / `RateLimitMaxBackoffMs` | `1000` / `30000` | 429 backoff bounds |

## Rate limiting

The server enforces a per-user limit (`general-authenticated`, ~100 req/min).
The worker catches HTTP 429 and retries with exponential backoff + jitter,
counting throttling events in the metrics. For high-throughput load tests you
can disable rate limiting on the server (`RateLimiting:Enabled=false`) and/or
raise `Operators` to spread load across more per-user buckets.

## Notes

- The worker **writes real data** to whatever API it targets. Each run uses a
  unique run tag so repeated runs don't collide on unique names.
- Only the entities created during a run are tracked in memory; the worker does
  not clean up after itself (it is a history generator).
- Both tracking modes are supported: entity-tracked articles get concrete items
  and containment moves; event-tracked articles get movement/lifecycle events
  via `POST /v1/item/events`.
