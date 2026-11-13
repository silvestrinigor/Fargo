# CLI Getting Started

This guide takes you from installing the CLI to running commands against a
Fargo server. Examples refer to the client as `fargo`.

## 1. Install the CLI

The CLI ships as a self-contained executable for Windows (x64) and Linux (x64),
so no .NET runtime is needed. The installer puts `fargo` on your `PATH`.

On Windows (PowerShell):

```powershell
irm https://raw.githubusercontent.com/silvestrinigor/Fargo/master/scripts/install.ps1 | iex
```

On Linux:

```bash
curl -fsSL https://raw.githubusercontent.com/silvestrinigor/Fargo/master/scripts/install.sh | sh
```

Open a new terminal, then check the install:

```bash
fargo --version
```

For specific versions, uninstalling, and manual installs, see
[Installing the CLI](https://github.com/silvestrinigor/Fargo#installing-the-cli)
in the repository README.

If you work from a checkout instead, run the project from the repo root. This
does not put `fargo` on your `PATH`, so replace `fargo` in the examples with
the `dotnet run` form:

```bash
dotnet run --project src/Fargo.Cli -- <command>
```

## 2. Point the CLI at a server

Set and persist the server the CLI connects to by default:

```bash
fargo config set-server --url https://localhost:7563
fargo config show
```

## 3. Log in

Authenticate. Omit `--password` to be prompted securely (the input is not
echoed):

```bash
fargo identity login --username admin
```

The session token is stored in your OS credential store and reused by later
commands. It is bound to the server you logged in against. On Linux this needs
the Secret Service (see
[Authentication and sessions](overview.md#authentication-and-sessions)).

## 4. Work with resources

List resources as a table:

```bash
fargo article list
fargo item list --page 1 --limit 20
```

Inspect a single resource:

```bash
fargo article get 11111111-1111-1111-1111-111111111111
fargo user get 00000000-0000-0000-0000-000000000004
```

Create, update, and delete:

```bash
fargo article create --name "Widget"
fargo article create --name "Bulk sand" --tracking-mode event
fargo partition create --name "Warehouse A"
fargo user set-active 00000000-0000-0000-0000-000000000004 false
fargo item delete 22222222-2222-2222-2222-222222222222
```

## 5. Inspect the audit trail

Every create, update, and delete is recorded. List recent entries, then fetch
one with its metadata:

```bash
fargo audit list --limit 20
fargo audit get 809434f3-c3b6-4f74-ad15-74e41d26468e
```

## 6. Watch changes live

Print changes as they happen, until Ctrl+C. The real-time service has its own
URL; a fresh configuration defaults to `https://localhost:7571`:

```bash
fargo config set-realtime-server --url https://localhost:7571
fargo realtime watch
```

See [Watching live changes](overview.md#watching-live-changes) for the output
and options.

## Ad-hoc flows

Override the server for a single command without changing your configuration:

```bash
fargo article list --server https://staging.example.com
```

Authenticate as a different user for a single command without changing your
stored session (resource commands only):

```bash
fargo user list --username auditor --password '<password>'
```

## Troubleshooting

- **`401 Unauthorized`** — run `fargo identity login` again, or pass
  `--username`/`--password`. Expired access tokens refresh automatically; an
  expired refresh token requires a new login.
- **`403 Forbidden`** — your account lacks permission for that action.
- Unsure about a command's options? Append `--help`:

  ```bash
  fargo item set-partitions --help
  ```

## Next steps

See the [Command Reference](reference/index.md) for every command and option.
