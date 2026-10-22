# User Group

Command reference for `fargo user-group`, generated from the CLI's help output.

## `fargo user-group`

```text
Description:
  Manage user groups - collections of users that share permissions.

Usage:
  fargo user-group [command] [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

Commands:
  list           List user groups as a table, with optional paging and partition filters.
  
                 Examples:
                   fargo user-group list
                   fargo user-group list --page 1 --limit 20
  tree           Show user groups as a tree: child groups nested under their parent,
                 and member users nested under each group.
  
                 Each line is tagged with its type ([group] or [user]). Users that
                 are not in any group appear at the root level, alongside the
                 top-level groups.
  
                 Examples:
                   fargo user-group tree
  get <guid>     Show the full details of a single user group.
  
                 Examples:
                   fargo user-group get 00000000-0000-0000-0000-000000000003
  create         Create a new user group, optionally nested under a parent group.
  
                 Examples:
                   fargo user-group create --name warehouse-ops
                   fargo user-group create --name warehouse-ops --parent 00000000-0000-0000-0000-000000000003
  update <guid>  Update an existing user group. Only the options you pass are changed.
  
                 Examples:
                   fargo user-group update 00000000-0000-0000-0000-000000000003 --active false
                   fargo user-group update 00000000-0000-0000-0000-000000000003 --remove-parent
  delete <guid>  Delete a user group by GUID.
  
                 Examples:
                   fargo user-group delete 00000000-0000-0000-0000-000000000003

```

## `fargo user-group list`

```text
Description:
  List user groups as a table, with optional paging and partition filters.
  
  Examples:
    fargo user-group list
    fargo user-group list --page 1 --limit 20

Usage:
  fargo user-group list [options]

Options:
  --page <page>              Page number to retrieve (starts at 1).
  --limit <limit>            Maximum number of user groups per page.
  --partitions <partitions>  Only show user groups under these partition GUIDs (space-separated).
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose              Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user-group tree`

```text
Description:
  Show user groups as a tree: child groups nested under their parent,
  and member users nested under each group.
  
  Each line is tagged with its type ([group] or [user]). Users that
  are not in any group appear at the root level, alongside the
  top-level groups.
  
  Examples:
    fargo user-group tree

Usage:
  fargo user-group tree [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user-group get`

```text
Description:
  Show the full details of a single user group.
  
  Examples:
    fargo user-group get 00000000-0000-0000-0000-000000000003

Usage:
  fargo user-group get <guid> [options]

Arguments:
  <guid>  The user group GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user-group create`

```text
Description:
  Create a new user group, optionally nested under a parent group.
  
  Examples:
    fargo user-group create --name warehouse-ops
    fargo user-group create --name warehouse-ops --parent 00000000-0000-0000-0000-000000000003

Usage:
  fargo user-group create [options]

Options:
  --name <name> (REQUIRED)  Name identifier (nameid) for the user group.
  --active                  Whether the group is active. Defaults to true.
  --parent <parent>         GUID of the parent user group.
  --username <username>     Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>     Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help            Show help and usage information
  --server <server>         The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose             Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user-group update`

```text
Description:
  Update an existing user group. Only the options you pass are changed.
  
  Examples:
    fargo user-group update 00000000-0000-0000-0000-000000000003 --active false
    fargo user-group update 00000000-0000-0000-0000-000000000003 --remove-parent

Usage:
  fargo user-group update <guid> [options]

Arguments:
  <guid>  The user group GUID.

Options:
  --active               Set the user group active state (true or false).
  --parent <parent>      New parent user group GUID.
  --remove-parent        Remove the parent user group association.
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user-group delete`

```text
Description:
  Delete a user group by GUID.
  
  Examples:
    fargo user-group delete 00000000-0000-0000-0000-000000000003

Usage:
  fargo user-group delete <guid> [options]

Arguments:
  <guid>  The user group GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

