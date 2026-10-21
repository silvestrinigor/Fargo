# Audit

Command reference for `fargo audit`, generated from the CLI's help output.

## `fargo audit`

```text
Description:
  View audit logs - a record of actions performed in the system.

Usage:
  fargo audit [command] [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

Commands:
  list        List audit log entries as a table, most useful with paging.
  
              Each row shows who (actor) did what (action) to which entity, and when.
  
              Examples:
                fargo audit list
                fargo audit list --page 1 --limit 50
  get <guid>  Show a single audit log by GUID, including its metadata.
  
              Examples:
                fargo audit get 809434f3-c3b6-4f74-ad15-74e41d26468e

```

## `fargo audit list`

```text
Description:
  List audit log entries as a table, most useful with paging.
  
  Each row shows who (actor) did what (action) to which entity, and when.
  
  Examples:
    fargo audit list
    fargo audit list --page 1 --limit 50

Usage:
  fargo audit list [options]

Options:
  --page <page>          Page number to retrieve (starts at 1).
  --limit <limit>        Maximum number of audit entries per page.
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo audit get`

```text
Description:
  Show a single audit log by GUID, including its metadata.
  
  Examples:
    fargo audit get 809434f3-c3b6-4f74-ad15-74e41d26468e

Usage:
  fargo audit get <guid> [options]

Arguments:
  <guid>  The audit log GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

