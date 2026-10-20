# Getting Started

This guide walks through running Fargo locally.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A container runtime (Docker or Podman) — the application uses
  [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) to orchestrate a
  PostgreSQL database.

## Running the application

From the repository root:

```bash
dotnet run --project src/server/Fargo.AppHost
```

The Aspire AppHost starts the services in order:

1. PostgreSQL and the `fargo` database.
2. The migration service (applies pending database migrations).
3. The seed service (creates the default administrator on first run).
4. The HTTP API.

See [Server Architecture](server/architecture.md#startup-and-orchestration) for the full startup graph.

Once the HTTP API is running, its base address is shown in the Aspire
dashboard (by default `https://localhost:7563`).

## Running the tests

```bash
dotnet test
```

## Default administrator

On first run the seed service creates a default administrator account from the
`DefaultAdmin` configuration section. In development this is `admin`. Use it to
obtain an access token and then create additional users and groups.

## Talking to the API

- Point any HTTP client at the server's base address and authenticate against
  the [Identity endpoints](web-api/overview.md#authentication).
- Or use the `fargo` command-line client, which stores your session token and
  targets a configurable server.

Continue with the [Web API Overview](web-api/overview.md).
