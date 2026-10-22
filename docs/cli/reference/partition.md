# Partition

Command reference for `fargo partition`, generated from the CLI's help output.

## `fargo partition`

```text
Description:
  Manage partitions - the hierarchy used to scope and group data.

Usage:
  fargo partition [command] [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

Commands:
  list           List partitions as a table, with optional paging and parent filters.
  
                 Examples:
                   fargo partition list
                   fargo partition list --page 1 --limit 20
                   fargo partition list --partitions 00000000-0000-0000-0000-000000000001
  tree           Show partitions as a tree: child partitions nested under their parent.
  
                 Each line is tagged with its type ([partition]). Partitions with no
                 parent (or whose parent is not visible) appear at the root level.
  
                 Examples:
                   fargo partition tree
  get <guid>     Show the full details of a single partition.
  
                 Examples:
                   fargo partition get 00000000-0000-0000-0000-000000000001
  create         Create a new partition, optionally nested under a parent.
  
                 Examples:
                   fargo partition create --name "Warehouse A"
                   fargo partition create --name "Aisle 1" --parent 00000000-0000-0000-0000-000000000001
  update <guid>  Update an existing partition. Only the options you pass are changed.
  
                 Examples:
                   fargo partition update 00000000-0000-0000-0000-000000000002 --parent 00000000-0000-0000-0000-000000000001
  delete <guid>  Delete a partition by GUID.
  
                 Examples:
                   fargo partition delete 00000000-0000-0000-0000-000000000002

```

## `fargo partition list`

```text
Description:
  List partitions as a table, with optional paging and parent filters.
  
  Examples:
    fargo partition list
    fargo partition list --page 1 --limit 20
    fargo partition list --partitions 00000000-0000-0000-0000-000000000001

Usage:
  fargo partition list [options]

Options:
  --page <page>              Page number to retrieve (starts at 1).
  --limit <limit>            Maximum number of partitions per page.
  --partitions <partitions>  Only show partitions under these parent GUIDs (space-separated).
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose              Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo partition tree`

```text
Description:
  Show partitions as a tree: child partitions nested under their parent.
  
  Each line is tagged with its type ([partition]). Partitions with no
  parent (or whose parent is not visible) appear at the root level.
  
  Examples:
    fargo partition tree

Usage:
  fargo partition tree [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo partition get`

```text
Description:
  Show the full details of a single partition.
  
  Examples:
    fargo partition get 00000000-0000-0000-0000-000000000001

Usage:
  fargo partition get <guid> [options]

Arguments:
  <guid>  The partition GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo partition create`

```text
Description:
  Create a new partition, optionally nested under a parent.
  
  Examples:
    fargo partition create --name "Warehouse A"
    fargo partition create --name "Aisle 1" --parent 00000000-0000-0000-0000-000000000001

Usage:
  fargo partition create [options]

Options:
  --name <name> (REQUIRED)  Name of the partition.
  --parent <parent>         GUID of the parent partition.
  --username <username>     Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>     Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help            Show help and usage information
  --server <server>         The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose             Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo partition update`

```text
Description:
  Update an existing partition. Only the options you pass are changed.
  
  Examples:
    fargo partition update 00000000-0000-0000-0000-000000000002 --parent 00000000-0000-0000-0000-000000000001

Usage:
  fargo partition update <guid> [options]

Arguments:
  <guid>  The partition GUID.

Options:
  --parent <parent>      New parent partition GUID.
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo partition delete`

```text
Description:
  Delete a partition by GUID.
  
  Examples:
    fargo partition delete 00000000-0000-0000-0000-000000000002

Usage:
  fargo partition delete <guid> [options]

Arguments:
  <guid>  The partition GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

