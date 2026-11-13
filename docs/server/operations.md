# Operations

This page covers running and configuring the server.

## Running locally

The server is orchestrated with .NET Aspire, which needs a container runtime
(Docker or Podman). From the repository root:

```bash
dotnet run --project src/Fargo.AppHost
```

The AppHost starts persistent PostgreSQL and RabbitMQ containers, then the hosts
in dependency order:

1. **PostgreSQL** with the `fargo` database, and **RabbitMQ** with its
   management plugin.
2. **Migration service**: applies pending migrations, then exits.
3. **Initializer service**: waits for the migrations, sets up the initial system state on
   first run, then exits.
4. **HTTP API**: waits for migration and the initializer, then serves requests.
5. **Event publisher**: waits for the migrations and RabbitMQ, then publishes
   integration events.
6. **Real-time service**: waits for the migrations. It does not wait for
   RabbitMQ; its consumer connects when the broker is up.
7. **Maintenance service**: waits for the migrations, then deletes expired
   idempotency keys and published outbox messages on a timer. It needs only the
   database, so it runs without RabbitMQ.

The Aspire dashboard shows each resource, its logs and its address. The API
defaults to `https://localhost:7563` and the real-time service to
`https://localhost:7571`. See
[Architecture](architecture.md#startup-and-orchestration) for the startup graph.

## Services

| Service | Project | Responsibility |
| --- | --- | --- |
| API | `Fargo.Api` | Serves the HTTP API. |
| Real-time | `Fargo.Realtime` | Pushes changes to connected clients. See [Real-time Updates](realtime.md). |
| Event publisher | `Fargo.OutboxRelay` | Publishes integration events to RabbitMQ. See [Integration Events](integration-events.md). |
| Maintenance | `Fargo.Maintenance` | Deletes expired idempotency keys and published outbox messages on a timer. See [Idempotency](idempotency.md) and [Integration Events](integration-events.md#configuration). |
| Migration | `Fargo.Migrator` | Applies EF Core migrations at startup, then exits. |
| Initializer | `Fargo.Initializer` | Initializes the system state at startup, then exits. |

The migration and initializer services are run-to-completion workers: they do their job
and stop, which is why the other services wait for their completion before
starting.

## Configuration

Configuration uses standard .NET sources (`appsettings.json`,
`appsettings.{Environment}.json`, environment variables).

### Connection strings

| Name | Used by | Under Aspire |
| --- | --- | --- |
| `fargo-database` | Every service | Supplied from the PostgreSQL resource. |
| `fargo-rabbitmq` | Event publisher, real-time service | Supplied from the RabbitMQ resource. |

### Authentication

JWT settings are bound from the `Jwt` section into `JwtOptions` and validated at
startup. The API and the real-time service must use the same settings, because
the real-time service validates the tokens the API issues. See
[Security](security.md#authentication).

### Settings by section

| Section | Service | Covered in |
| --- | --- | --- |
| `RateLimiting` | API | [Cross-Cutting Concerns](cross-cutting.md#rate-limiting) |
| `ForwardedHeaders` | API | [Behind a proxy](#behind-a-proxy) below |
| `ActorAuthorizationCache` | API, real-time | [Security](security.md#how-the-principal-is-read-and-cached) |
| `Outbox`, `RabbitMqEvents` | Event publisher | [Integration Events](integration-events.md#configuration) |
| `Idempotency` | API | [Idempotency](idempotency.md#configuration) |
| `Maintenance` | Maintenance | [Idempotency](idempotency.md#configuration), [Integration Events](integration-events.md#configuration) |
| `Realtime`, `RabbitMqConsumer` | Real-time | [Real-time Updates](realtime.md#configuration) |
| `GlobalPartition`, `Administrators`, `DefaultAdmin` | Initializer | [Initial system data](#initial-system-data) below |

### Initial system data

The initializer service reads the initial system values from configuration:

| Section | Purpose |
| --- | --- |
| `GlobalPartition` | Name and description of the root partition. |
| `Administrators` | Name and description of the administrators user group. |
| `DefaultAdmin` | Nameid, password, and description of the default administrator. |

In development these default to a `Global` partition, an `administrators` group,
and an `admin` user. **Supply non-default administrator credentials outside
development.**

### Behind a proxy

When the API runs behind a reverse proxy or load balancer, enable the
`ForwardedHeaders` section so the real client IP and scheme are used for HTTPS
redirection, authentication and rate limiting:

| Setting | Default | Meaning |
| --- | --- | --- |
| `Enabled` | `false` | Read `X-Forwarded-For` and `X-Forwarded-Proto`. |
| `KnownProxies` | empty | IP addresses of trusted proxies. |
| `KnownNetworks` | empty | Trusted networks in CIDR form, for example `10.0.0.0/8`. |

With neither list set, any peer is trusted. Only do that when the API can be
reached solely through the proxy.

### Telemetry

Set `OTEL_EXPORTER_OTLP_ENDPOINT` to export logs, metrics, and traces via OTLP.
See [Cross-Cutting Concerns](cross-cutting.md#telemetry-opentelemetry).

## Health

The API and the real-time service map two endpoints in every environment:

| Endpoint | Meaning |
| --- | --- |
| `/health` | Ready: all checks pass, including the database check. |
| `/alive` | Live: the process is responsive. |

They return only the overall status, no component details. If a service is
exposed publicly, restrict these endpoints at the proxy or network layer.

## Environments

`ASPNETCORE_ENVIRONMENT` selects the environment. In **Development** the API
maps the OpenAPI document; outside development it enables HSTS instead.
