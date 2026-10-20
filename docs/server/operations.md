# Operations

This page covers running and configuring the server.

## Running locally

The server is orchestrated with .NET Aspire. From the repository root:

```bash
dotnet run --project src/server/Fargo.AppHost
```

The AppHost starts a persistent PostgreSQL container and the service projects in
dependency order:

1. **PostgreSQL** and the `fargo` database.
2. **Migration service** — waits for the database, then applies pending
   migrations.
3. **Seed service** — waits for migrations to complete, then seeds the initial
   system state on first run.
4. **HTTP API** — waits for both migration and seed to complete, then serves
   requests. It exposes an HTTP health check at `/health`.

The Aspire dashboard shows each resource, its logs, and the API's base address
(by default `https://localhost:7563`). See
[Architecture](architecture.md#startup-and-orchestration) for the startup graph.

## Services

| Service | Project | Responsibility |
| --- | --- | --- |
| API | `Fargo.ServiceHttp` | Serves the HTTP API. |
| Migration | `Fargo.ServiceMigration` | Applies EF Core migrations at startup, then exits. |
| Seed | `Fargo.ServiceSeed` | Seeds initial system state at startup, then exits. |

The migration and seed services are run-to-completion workers: they do their job
and stop, which is why the API waits for their completion before starting.

## Configuration

Configuration uses standard ASP.NET Core sources (`appsettings.json`,
`appsettings.{Environment}.json`, environment variables).

### Connection string

The database connection string is named `fargo-database`. Under Aspire it is
supplied automatically from the orchestrated PostgreSQL resource.

### Authentication

JWT settings are bound from the `Jwt` section into `JwtOptions` and validated at
startup. See [Security](security.md#authentication).

### Seed data

The seed service reads the initial system values from configuration:

| Section | Purpose |
| --- | --- |
| `GlobalPartition` | Name and description of the root partition. |
| `Administrators` | Name and description of the administrators user group. |
| `DefaultAdmin` | Nameid, password, and description of the default administrator. |

In development these default to a `Global` partition, an `administrators` group,
and an `admin` user. **Supply non-default administrator credentials outside
development.**

### Telemetry

Set `OTEL_EXPORTER_OTLP_ENDPOINT` to export logs, metrics, and traces via OTLP.
See [Cross-Cutting Concerns](cross-cutting.md#telemetry-opentelemetry).

## Health

| Endpoint | Meaning | Availability |
| --- | --- | --- |
| `/health` | Ready — all checks pass, including the database check. | Development only |
| `/alive` | Live — the process is responsive. | Development only |

Health endpoints are mapped only in development; exposing them elsewhere has
security implications and should be done deliberately.

## Environments

`ASPNETCORE_ENVIRONMENT` selects the environment. In **Development** the server
maps the OpenAPI document and the health endpoints; outside development it
enables HSTS and does not expose those endpoints by default.
