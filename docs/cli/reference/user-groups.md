# User Groups

Command reference for `fargo user-groups`, generated from the CLI's help output.

## `fargo user-groups`

```text
Description:
  Manage user groups - collections of users that share permissions.

Usage:
  Fargo.Cli user-groups [command] [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

Commands:
  list           List user groups as a table, with optional paging and partition filters.
  
                 Examples:
                   fargo user-groups list
                   fargo user-groups list --page 1 --limit 20
  get <guid>     Show the full details of a single user group.
  
                 Examples:
                   fargo user-groups get 00000000-0000-0000-0000-000000000003
  create         Create a new user group, optionally nested under a parent group.
  
                 Examples:
                   fargo user-groups create --name warehouse-ops
                   fargo user-groups create --name warehouse-ops --parent 00000000-0000-0000-0000-000000000003
  update <guid>  Update an existing user group. Only the options you pass are changed.
  
                 Examples:
                   fargo user-groups update 00000000-0000-0000-0000-000000000003 --active false
                   fargo user-groups update 00000000-0000-0000-0000-000000000003 --remove-parent
  delete <guid>  Delete a user group by GUID.
  
                 Examples:
                   fargo user-groups delete 00000000-0000-0000-0000-000000000003

```

## `fargo user-groups list`

```text
Description:
  List user groups as a table, with optional paging and partition filters.
  
  Examples:
    fargo user-groups list
    fargo user-groups list --page 1 --limit 20

Usage:
  Fargo.Cli user-groups list [options]

Options:
  --page <page>              Page number to retrieve (starts at 1).
  --limit <limit>            Maximum number of user groups per page.
  --partitions <partitions>  Only show user groups under these partition GUIDs (space-separated).
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo user-groups get`

```text
Description:
  Show the full details of a single user group.
  
  Examples:
    fargo user-groups get 00000000-0000-0000-0000-000000000003

Usage:
  Fargo.Cli user-groups get <guid> [options]

Arguments:
  <guid>  The user group GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo user-groups create`

```text
Description:
  Create a new user group, optionally nested under a parent group.
  
  Examples:
    fargo user-groups create --name warehouse-ops
    fargo user-groups create --name warehouse-ops --parent 00000000-0000-0000-0000-000000000003

Usage:
  Fargo.Cli user-groups create [options]

Options:
  --name <name> (REQUIRED)  Name identifier (nameid) for the user group.
  --active                  Whether the group is active. Defaults to true.
  --parent <parent>         GUID of the parent user group.
  --username <username>     Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>     Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help            Show help and usage information
  --server <server>         The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo user-groups update`

```text
Description:
  Update an existing user group. Only the options you pass are changed.
  
  Examples:
    fargo user-groups update 00000000-0000-0000-0000-000000000003 --active false
    fargo user-groups update 00000000-0000-0000-0000-000000000003 --remove-parent

Usage:
  Fargo.Cli user-groups update <guid> [options]

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

```

## `fargo user-groups delete`

```text
Description:
  Delete a user group by GUID.
  
  Examples:
    fargo user-groups delete 00000000-0000-0000-0000-000000000003

Usage:
  Fargo.Cli user-groups delete <guid> [options]

Arguments:
  <guid>  The user group GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

