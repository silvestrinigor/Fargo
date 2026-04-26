# Introduction

Fargo is a .NET 10 inventory and resource management system that tracks product definitions and physical stock across a hierarchical location tree. It exposes a REST API, a Blazor web UI, a typed .NET client SDK, and real-time mutation events via SignalR.

## Domain Model

### Article

An **Article** is the blueprint or definition of a product. It describes *what* a thing is — its name, description, physical dimensions (length, width, height), mass, image, and barcodes. An article can exist in zero or more partitions.

### Item

An **Item** is one specific physical instance of an Article. Where an article is the concept, an item is the individual unit. Items also belong to partitions and can be moved between them.

### Partition

A **Partition** is a hierarchical node in the location tree — a warehouse, a room, a rack, a shelf, or any other logical or physical grouping. Partitions can be nested arbitrarily deep. Articles and Items are assigned to partitions to express *where* they are.

### User

A **User** authenticates with a username (`nameid`) and password. Each user holds a set of `ActionType` permissions (e.g. `CreateArticle`, `EditUser`) and a list of partitions they are allowed to access. Users can also belong to UserGroups.

### UserGroup

A **UserGroup** is a named role that carries its own permission set and partition access list. Adding a user to a group grants them the union of their individual and group permissions.

### ApiClient

An **ApiClient** is a machine-to-machine identity. Clients authenticate with an API key sent in the `X-Api-Key` request header. They are useful for background services, scripts, and MCP tools that do not represent a human user.

## Architecture Layers

```
┌─────────────────────────────────────────────┐
│  Fargo.Api          REST endpoints + SignalR │
│  Fargo.Web          Blazor Server UI         │
│  Fargo.Sdk          Typed .NET client SDK    │
│  Fargo.Mcp          MCP tools                │
├─────────────────────────────────────────────┤
│  Fargo.Application  CQRS use cases           │
├─────────────────────────────────────────────┤
│  Fargo.Infrastructure  EF Core, security     │
├─────────────────────────────────────────────┤
│  Fargo.Domain       Entities, value objects  │
└─────────────────────────────────────────────┘
```

**Fargo.Domain** holds the core entities and value objects and has no dependencies outside the BCL.

**Fargo.Application** implements the use cases as CQRS command/query handlers. It depends only on domain abstractions.

**Fargo.Infrastructure** provides persistence (SQL Server via EF Core), password hashing, JWT generation, refresh-token management, and file storage.

**Fargo.Api** is the ASP.NET Core host. It maps HTTP endpoints to application commands/queries and publishes mutation events to connected SignalR clients via `FargoEventHub`.

**Fargo.Sdk** is the typed .NET client SDK. It wraps the REST API and the SignalR hub into easy-to-use manager interfaces. Live entity objects (e.g. `Article`, `Item`) fire .NET events when they are updated or deleted by any connected client.

**Fargo.Web** is a Blazor Server application that consumes `Fargo.Sdk` and provides the default user interface.

**Fargo.Mcp** exposes Fargo operations as Model Context Protocol tools for AI assistant integration.

## Authentication

Fargo supports two authentication mechanisms:

- **JWT bearer + refresh token** — the default for human users. The client logs in with a username and password, receives a short-lived access token and a longer-lived refresh token. The SDK refreshes the access token automatically when it expires.
- **API key** — intended for machine clients. The key is sent as the `X-Api-Key` HTTP header.

Users have fine-grained `ActionType` permissions. The full list is in the `ActionType` enum (`Fargo.Sdk.ActionType` / `Fargo.Domain.ActionType`). Permissions can be assigned directly to a user or inherited from a UserGroup.

## Real-time Events

The API broadcasts all mutation events (create, update, delete) to connected clients over a SignalR hub. The SDK connects to this hub automatically on login and routes events to individual live entity objects:

```csharp
// article fires .NET events when any client modifies it
article.Updated += (_, e) => Refresh();
article.Deleted += (_, e) => NavigateBack();
```

Top-level event sources expose `Created` events for new entities:

```csharp
engine.Articles.Created += (_, e) => Console.WriteLine($"New article: {e.Guid}");
```
