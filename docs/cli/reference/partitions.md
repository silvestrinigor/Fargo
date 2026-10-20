# Partitions

Command reference for `fargo partitions`, generated from the CLI's help output.

## `fargo partitions`

```text
Description:
  Manage partitions - the hierarchy used to scope and group data.

Usage:
  Fargo.Cli partitions [command] [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

Commands:
  list           List partitions as a table, with optional paging and parent filters.
  
                 Examples:
                   fargo partitions list
                   fargo partitions list --page 1 --limit 20
                   fargo partitions list --partitions 00000000-0000-0000-0000-000000000001
  get <guid>     Show the full details of a single partition.
  
                 Examples:
                   fargo partitions get 00000000-0000-0000-0000-000000000001
  create         Create a new partition, optionally nested under a parent.
  
                 Examples:
                   fargo partitions create --name "Warehouse A"
                   fargo partitions create --name "Aisle 1" --parent 00000000-0000-0000-0000-000000000001
  update <guid>  Update an existing partition. Only the options you pass are changed.
  
                 Examples:
                   fargo partitions update 00000000-0000-0000-0000-000000000002 --parent 00000000-0000-0000-0000-000000000001
  delete <guid>  Delete a partition by GUID.
  
                 Examples:
                   fargo partitions delete 00000000-0000-0000-0000-000000000002

```

## `fargo partitions list`

```text
Description:
  List partitions as a table, with optional paging and parent filters.
  
  Examples:
    fargo partitions list
    fargo partitions list --page 1 --limit 20
    fargo partitions list --partitions 00000000-0000-0000-0000-000000000001

Usage:
  Fargo.Cli partitions list [options]

Options:
  --page <page>              Page number to retrieve (starts at 1).
  --limit <limit>            Maximum number of partitions per page.
  --partitions <partitions>  Only show partitions under these parent GUIDs (space-separated).
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo partitions get`

```text
Description:
  Show the full details of a single partition.
  
  Examples:
    fargo partitions get 00000000-0000-0000-0000-000000000001

Usage:
  Fargo.Cli partitions get <guid> [options]

Arguments:
  <guid>  The partition GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo partitions create`

```text
Description:
  Create a new partition, optionally nested under a parent.
  
  Examples:
    fargo partitions create --name "Warehouse A"
    fargo partitions create --name "Aisle 1" --parent 00000000-0000-0000-0000-000000000001

Usage:
  Fargo.Cli partitions create [options]

Options:
  --name <name> (REQUIRED)  Name of the partition.
  --parent <parent>         GUID of the parent partition.
  --username <username>     Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>     Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help            Show help and usage information
  --server <server>         The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo partitions update`

```text
Description:
  Update an existing partition. Only the options you pass are changed.
  
  Examples:
    fargo partitions update 00000000-0000-0000-0000-000000000002 --parent 00000000-0000-0000-0000-000000000001

Usage:
  Fargo.Cli partitions update <guid> [options]

Arguments:
  <guid>  The partition GUID.

Options:
  --parent <parent>      New parent partition GUID.
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo partitions delete`

```text
Description:
  Delete a partition by GUID.
  
  Examples:
    fargo partitions delete 00000000-0000-0000-0000-000000000002

Usage:
  Fargo.Cli partitions delete <guid> [options]

Arguments:
  <guid>  The partition GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

