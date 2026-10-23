# CLI Overview

`fargo` is the command-line client for the Fargo inventory API. It wraps the
[Web API](~/docs/web-api/overview.md) with a convenient terminal interface,
stores your session token, and targets a configurable server.

For the full, generated list of commands and options, see the
[Command Reference](reference/index.md).

## Running the CLI

The CLI is the `Fargo.Cli` project:

```bash
dotnet run --project src/client/Fargo.Cli -- <command> [options]
```

In the examples throughout these docs the client is referred to as `fargo`
(the compiled executable name).

Run any command with `--help` to see its description, options, and examples:

```bash
fargo --help
fargo article --help
fargo article create --help
```

## Command groups

| Group | Description |
| --- | --- |
| `config` | Manage CLI configuration (the server URL the CLI connects to). |
| `identity` | Authenticate: login, logout, refresh, change password. |
| `articles` | Manage articles — the catalog of product definitions. |
| `items` | Manage items — physical instances of articles and their locations. |
| `partitions` | Manage partitions — the hierarchy used to scope and group data. |
| `user-groups` | Manage user groups — collections of users that share permissions. |
| `users` | Manage user accounts. |
| `audit` | View audit logs — a record of actions performed in the system. |

## Global options

| Option | Scope | Description |
| --- | --- | --- |
| `--server <url>` | Every command | Override the configured server for this one invocation. |
| `--username <name>` | Resource commands | Authenticate ad hoc for this one invocation (with `--password`). Not available on `identity` commands. |
| `--password <value>` | Resource commands | Paired with `--username`. Not available on `identity` commands. |

`--username`/`--password` let you run a single command as a given user without
changing your stored session. They are intentionally unavailable on the
`identity` commands, which manage authentication themselves.

## Authentication and sessions

- Authenticate once with `fargo identity login`; the access and refresh tokens
  are stored in your operating system's credential store.
- **Tokens are bound to the server they were issued for.** A token obtained
  against one server is never sent to another, so switching `--server` or the
  configured server will not reuse the wrong token.
- **Expired access tokens are refreshed automatically.** Before a command runs,
  if the stored access token has expired, the CLI refreshes it using the stored
  refresh token. If the refresh token is also expired, the command reports an
  authentication error and you log in again.
- `fargo identity login` can prompt for the password securely when `--password`
  is omitted, so the secret never appears in your shell history.

## Output

Human-friendly, plain-text output:

- **List** commands render an aligned ASCII table with a header and a result
  count, or a friendly "No results." message when empty.
- **Get** commands render an aligned key/value detail block.
- **Create / update / delete** commands print a concise confirmation.
- **Enumerations are shown by name**, not by number — for example an audit log
  type is shown as `entity-created` rather than `1`.

## Errors

API errors are rendered as concise messages instead of stack traces:

- `401 Unauthorized` — you are not authenticated or your session expired. Run
  `fargo identity login`, or provide `--username`/`--password`.
- `403 Forbidden` — you are authenticated but lack permission for the action.

When the server returns problem details, the message includes the server's
explanation and a trace id for support.

## Next steps

- [Getting Started](getting-started.md) — point the CLI at a server and run your
  first commands.
- [Command Reference](reference/index.md) — every command and option.
