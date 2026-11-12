# MCP Server Overview

`Fargo.Mcp` is a [Model Context Protocol](https://modelcontextprotocol.io) (MCP)
server that exposes the Fargo API as a set of tools an AI agent can call. It lets
MCP-capable hosts — such as Claude Desktop, Claude Code, Cursor, or VS Code's MCP
client — list, read, and manage Fargo articles, items, item events, partitions,
users, user groups, and audit logs through natural-language-driven tool calls.

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
| Articles | `fargo_articles_list`, `fargo_articles_get`, `fargo_articles_get_by_barcode`, `fargo_articles_create`, `fargo_articles_update`, `fargo_articles_delete`, `fargo_articles_get_inventory` | Read/write |
| Items | `fargo_items_list`, `fargo_items_get`, `fargo_items_create`, `fargo_items_update`, `fargo_items_move`, `fargo_items_remove_from_container`, `fargo_items_delete`, `fargo_items_get_location`, `fargo_items_list_moviments`, `fargo_items_get_inventory` | Read/write |
| Item events | `fargo_item_events_list`, `fargo_item_events_create`, `fargo_item_events_delete`, `fargo_item_events_move` | Read/write |
| Partitions | `fargo_partitions_list`, `fargo_partitions_get`, `fargo_partitions_create`, `fargo_partitions_update`, `fargo_partitions_set_parent`, `fargo_partitions_delete` | Read/write |
| Users | `fargo_users_list`, `fargo_users_get`, `fargo_users_create`, `fargo_users_update`, `fargo_users_set_active`, `fargo_users_set_permissions`, `fargo_users_set_user_groups`, `fargo_users_set_partitions`, `fargo_users_reset_password`, `fargo_users_set_password_expiration`, `fargo_users_remove_password_expiration`, `fargo_users_delete` | Read/write |
| User groups | `fargo_usergroups_list`, `fargo_usergroups_get`, `fargo_usergroups_create`, `fargo_usergroups_update`, `fargo_usergroups_set_active`, `fargo_usergroups_set_permissions`, `fargo_usergroups_set_partitions`, `fargo_usergroups_set_parent`, `fargo_usergroups_remove_parent`, `fargo_usergroups_delete` | Read/write |
| Service accounts | `fargo_serviceaccounts_list`, `fargo_serviceaccounts_get`, `fargo_serviceaccounts_create`, `fargo_serviceaccounts_update`, `fargo_serviceaccounts_set_active`, `fargo_serviceaccounts_set_permissions`, `fargo_serviceaccounts_set_partitions`, `fargo_serviceaccounts_rotate_secret`, `fargo_serviceaccounts_delete` | Read/write |
| Audit logs | `fargo_auditlogs_list`, `fargo_auditlogs_get` | Read-only |

The list tools for articles, items, partitions, users and user groups accept
`search` and `sort` in addition to paging. Article update, delete and inventory
are addressed by GUID; use `fargo_articles_get_by_barcode` to resolve a barcode
to a GUID first.

> **Secrets.** `fargo_serviceaccounts_create` and
> `fargo_serviceaccounts_rotate_secret` return the new client secret once, in the
> tool result, so it passes through the model's context and the host's logs. The
> user tools also take passwords as arguments, including `fargo_users_reset_password`. Only approve these tools in
> sessions you trust.

Each tool's parameters and description are declared on the tool itself and
surfaced live by the MCP host, so the host's own tool list is the authoritative
reference for the current tool surface.

## Where to go next

- [Getting Started](getting-started.md) — configure and register the server with
  an MCP host.
