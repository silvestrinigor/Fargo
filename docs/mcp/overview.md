# MCP Server Overview

`Fargo.Mcp` is a [Model Context Protocol](https://modelcontextprotocol.io) (MCP)
server that exposes the Fargo API as a set of tools an AI agent can call. It lets
MCP-capable hosts — such as Claude Desktop, Claude Code, Cursor, or VS Code's MCP
client — list, read, and manage Fargo articles, items, partitions, users, user
groups, and audit logs through natural-language-driven tool calls.

Under the hood it is a thin layer over the same strongly-typed
[`Fargo.ClientHttp`](~/api/Fargo.ClientHttp.FargoApiClient.yml) Kiota client that
the `fargo` CLI uses, so the MCP tools behave consistently with the CLI and the
Web API.

## How it works

- The server runs as a console application that speaks the MCP protocol over
  **stdio** (standard input/output). The host launches the process and
  communicates with it over those streams.
- All diagnostic logging is written to **stderr** so it never corrupts the MCP
  protocol stream on stdout.
- Configuration — the target server URL and credentials — is supplied entirely
  through **environment variables**, because the host starts the process on
  demand.
- On the first tool call the server authenticates against the Fargo API (or uses
  a supplied access token) and keeps the token refreshed for the lifetime of the
  session.

## Tool surface

Tools are grouped by resource and named `fargo_<resource>_<action>`:

| Resource | Tools | Access |
| --- | --- | --- |
| Articles | `fargo_articles_list`, `fargo_articles_get_by_barcode`, `fargo_articles_create`, `fargo_articles_delete_by_barcode` | Read/write |
| Items | `fargo_items_list`, `fargo_items_get`, `fargo_items_create`, `fargo_items_delete` | Read/write |
| Partitions | `fargo_partitions_list`, `fargo_partitions_get`, `fargo_partitions_create`, `fargo_partitions_delete` | Read/write |
| Users | `fargo_users_list`, `fargo_users_get` | Read-only |
| User groups | `fargo_usergroups_list`, `fargo_usergroups_get` | Read-only |
| Audit logs | `fargo_auditlogs_list` | Read-only |

See the [Tools Reference](tools.md) for the parameters and return shape of each
tool.

## Where to go next

- [Getting Started](getting-started.md) — configure and register the server with
  an MCP host.
- [Tools Reference](tools.md) — the full list of tools and their parameters.
