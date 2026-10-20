# Introduction

Fargo is an inventory management system. It models the physical world of a
warehouse or stockroom as a small set of connected concepts and exposes them
through a REST API and a command-line client.

> [!NOTE]
> Fargo is under active development and is not yet production-ready. Features,
> APIs, database structures, and internal architecture may change without
> notice.

## Core concepts

| Concept | Description |
| --- | --- |
| **Article** | A product definition in the catalog (the "what"). Articles have a type, optional barcodes, dimensions, and other attributes. |
| **Item** | A physical instance of an article (the "thing on the shelf"). Items can be placed inside container items and moved between them. |
| **Partition** | A node in a hierarchy used to scope and group data. Access to data is granted through partitions. |
| **User** | An account that authenticates and acts in the system. |
| **User group** | A collection of users that share permissions. |
| **Audit log** | An immutable record of a change (entity created, updated, or deleted), including who did it and when. |

## How the pieces fit together

- Articles define *what* can exist; items are the *actual* units of those
  articles.
- Partitions scope *who can see what*. Users and user groups are granted access
  to partitions, and every article, item, and audit log is associated with one
  or more partitions.
- Permissions are granted to users and user groups and control *what actions*
  an actor may perform.
- Every create, update, and delete produces an audit log scoped to the same
  partitions as the affected entity.

## Ways to use Fargo

- **[Web API](web-api/overview.md)** — a REST API for integrating Fargo with
  other systems.
- **Command-line client (`fargo`)** — a terminal client for operators and
  scripts.
- **[MCP server](mcp/overview.md)** — a Model Context Protocol server that lets
  AI agents work with Fargo through tools.

## Where to go next

- [Getting Started](getting-started.md) — run Fargo locally.
- [Server](server/overview.md) — how the server is built: architecture, domain model, persistence, and security.
- [CLI](cli/overview.md) — the command-line client.
- [Web API Overview](web-api/overview.md) — the REST API surface.
- [MCP Server Overview](mcp/overview.md) — the Model Context Protocol server for AI agents.
- [API Reference](~/api/index.md) — the generated .NET API documentation.
