# Getting Started

This guide walks through running Fargo locally.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A container runtime (Docker or Podman). The application uses
  [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) to run PostgreSQL
  and RabbitMQ in containers.

## Running the application

From the repository root:

```bash
dotnet run --project src/Fargo.AppHost
```

The Aspire AppHost starts the services in order:

1. PostgreSQL with the `fargo` database, and RabbitMQ.
2. The migration service (applies pending database migrations).
3. The initializer service (creates the default administrator on first run).
4. The HTTP API, the event publisher (publishes integration events) and the
   real-time service (pushes changes to connected clients).

See [Operations](server/operations.md#running-locally) for the details and
[Architecture](server/architecture.md#startup-and-orchestration) for the
startup graph.

The Aspire dashboard shows each service's address. By default the HTTP API is
at `https://localhost:7563` and the real-time service at
`https://localhost:7571`.

## Running the tests

```bash
dotnet test
```

The tests that use a real PostgreSQL database or RabbitMQ broker start them in
containers, so they need the container runtime too. The rest run without it.

## Default administrator

On first run the initializer service creates a default administrator account from the
`DefaultAdmin` configuration section. In development this is `admin`. Use it to
obtain an access token and then create additional users and groups.

## Talking to the API

- Point any HTTP client at the server's base address and authenticate against
  the [Identity endpoints](web-api/overview.md#authentication).
- Or use the `fargo` command-line client, which stores your session token and
  targets a configurable server. See [CLI Getting Started](cli/getting-started.md).

## Next steps

- [Web API Overview](web-api/overview.md): the REST API.
- [Concepts](concepts/overview.md): what articles, items and partitions are.
