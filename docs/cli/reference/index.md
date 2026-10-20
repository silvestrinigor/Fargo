# Command Reference

This reference is generated from the `fargo` CLI's own `--help` output, so it
always matches the shipped commands. Select a group from the table of contents,
or browse the full command tree below.

> [!NOTE]
> Every command also accepts `--help` at the terminal to show the same
> information shown here.

## Command tree

```text
fargo
├─ config         Manage CLI configuration
│  ├─ set-server
│  ├─ get-server
│  └─ show
├─ identity       Authenticate with the Fargo server
│  ├─ login
│  ├─ logout
│  ├─ refresh
│  └─ change-password
├─ articles       Manage articles
│  ├─ list
│  ├─ get
│  ├─ get-by-barcode
│  ├─ inventory
│  ├─ create
│  ├─ update
│  └─ delete
├─ items          Manage items
│  ├─ list
│  ├─ get
│  ├─ create
│  ├─ update
│  ├─ delete
│  ├─ location
│  ├─ moviments
│  └─ inventory
├─ partitions     Manage partitions
│  ├─ list
│  ├─ get
│  ├─ create
│  ├─ update
│  └─ delete
├─ user-groups    Manage user groups
│  ├─ list
│  ├─ get
│  ├─ create
│  ├─ update
│  └─ delete
├─ users          Manage user accounts
│  ├─ list
│  ├─ get
│  ├─ create
│  ├─ update
│  └─ delete
└─ audit          View audit logs
   ├─ list
   └─ get
```

## Global options

These are available in addition to each command's own options:

- `--server <url>` — override the configured server for this invocation
  (available on every command).
- `--username <name>` / `--password <value>` — authenticate ad hoc for this
  invocation (resource commands only, not the `identity` commands).

See the [Overview](../overview.md) for how authentication, token scoping, and
output work.
