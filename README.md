# Fargo

<img src="doc/under_construction.png" width="400" alt="Under construction" />

Fargo is a .NET 10 inventory and resource management system. It tracks **Articles** (product definitions), **Items** (physical instances of articles), and their location within a hierarchical **Partition** tree, while managing **Users**, **UserGroups**, and **ApiClients** with fine-grained permission control and real-time event broadcasting.

## Domain Model

| Entity | Purpose |
|--------|---------|
| **Article** | Blueprint or definition of a product, including dimensions, mass, image, and barcodes |
| **Item** | A specific physical instance of an Article |
| **Partition** | Hierarchical location or category (warehouse, shelf, zone…) used to organize Articles and Items |
| **User** | System user with configurable action permissions and partition access |
| **UserGroup** | Group of users sharing a common permission set |
| **ApiClient** | Machine-to-machine identity authenticated via API key |

## Technology Stack

| Concern | Technology |
|---------|-----------|
| Language / Runtime | C# / .NET 10 |
| Web API | ASP.NET Core (minimal endpoints) |
| Web UI | Blazor Server (Razor components) |
| Database | SQL Server via Entity Framework Core 10 |
| Authentication | JWT bearer + refresh tokens |
| Real-time | SignalR |
| Orchestration | .NET Aspire |
| Documentation | DocFX |

## Architecture

The solution follows Clean Architecture with Domain-Driven Design principles:

```
Fargo.Domain          — Entities, value objects, domain services
Fargo.Application     — CQRS commands and queries (use cases)
Fargo.Infrastructure  — EF Core persistence, security, file storage
Fargo.Api             — ASP.NET Core REST API + SignalR hub
Fargo.Sdk             — Client SDK for consuming the API
Fargo.Web             — Blazor web application (uses Fargo.Sdk)
Fargo.Mcp             — Model Context Protocol tools
Fargo.AppHost         — .NET Aspire orchestration host
```

C4 architecture diagrams are in `doc/architecture/c4/diagrams/`.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/) (SQL Server container is managed by Aspire)

## Running the Application

```bash
dotnet run --project src/Fargo.AppHost
```

The Aspire dashboard opens automatically and shows all running services (SQL Server, API, Web). The web frontend URL is printed in the Aspire dashboard.

## Running Tests

```bash
dotnet test
```

## Documentation

API reference and conceptual documentation are generated with DocFX:

```bash
dotnet docfx docfx.json          # build
dotnet docfx serve _site         # serve locally at http://localhost:8080
```

## License

This project is currently under development and does not yet have a public license.

All rights reserved.

A license will be defined once the project reaches a stable state.
