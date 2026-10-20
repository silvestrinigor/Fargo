# CLI Getting Started

This guide takes you from a fresh checkout to running commands against a Fargo
server. Examples refer to the client as `fargo`; you can also run it with
`dotnet run --project src/client/Fargo.Cli -- <command>`.

## 1. Point the CLI at a server

Set and persist the server the CLI connects to by default:

```bash
fargo config set-server --url https://localhost:7563
fargo config show
```

## 2. Log in

Authenticate. Omit `--password` to be prompted securely (the input is not
echoed):

```bash
fargo identity login --username admin
```

The session token is stored in your OS credential store and reused by later
commands. It is bound to the server you logged in against.

## 3. Work with resources

List resources as a table:

```bash
fargo articles list
fargo items list --page 1 --limit 20
```

Inspect a single resource:

```bash
fargo articles get 11111111-1111-1111-1111-111111111111
fargo users get 00000000-0000-0000-0000-000000000004
```

Create, update, and delete:

```bash
fargo articles create --name "Widget" --type pack
fargo partitions create --name "Warehouse A"
fargo users update 00000000-0000-0000-0000-000000000004 --active false
fargo items delete 22222222-2222-2222-2222-222222222222
```

## 4. Inspect the audit trail

Every create, update, and delete is recorded. List recent entries, then fetch
one with its metadata:

```bash
fargo audit list --limit 20
fargo audit get 809434f3-c3b6-4f74-ad15-74e41d26468e
```

## Ad-hoc flows

Override the server for a single command without changing your configuration:

```bash
fargo articles list --server https://staging.example.com
```

Authenticate as a different user for a single command without changing your
stored session (resource commands only):

```bash
fargo users list --username auditor --password '<password>'
```

## Troubleshooting

- **`401 Unauthorized`** — run `fargo identity login` again, or pass
  `--username`/`--password`. Expired access tokens refresh automatically; an
  expired refresh token requires a new login.
- **`403 Forbidden`** — your account lacks permission for that action.
- Unsure about a command's options? Append `--help`:

  ```bash
  fargo items update --help
  ```

## Next steps

See the [Command Reference](reference/index.md) for every command and option.
