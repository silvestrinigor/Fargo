# Users

Command reference for `fargo users`, generated from the CLI's help output.

## `fargo users`

```text
Description:
  Manage user accounts (list, inspect, create, update, delete).

Usage:
  Fargo.Cli users [command] [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

Commands:
  list           List users as a table, with optional paging and partition filters.
  
                 Examples:
                   fargo users list
                   fargo users list --page 1 --limit 20
                   fargo users list --partitions 00000000-0000-0000-0000-000000000001
  get <guid>     Show the full details of a single user.
  
                 Examples:
                   fargo users get 00000000-0000-0000-0000-000000000004
  create         Create a new user account.
  
                 Examples:
                   fargo users create --username jdoe --password 'S3cr3t!'
                   fargo users create --username jdoe --password 'S3cr3t!' --first-name John --last-name Doe
  update <guid>  Update an existing user. Only the options you pass are changed.
  
                 Examples:
                   fargo users update 00000000-0000-0000-0000-000000000004 --first-name Jane
                   fargo users update 00000000-0000-0000-0000-000000000004 --active false
  delete <guid>  Delete a user by GUID.
  
                 Examples:
                   fargo users delete 00000000-0000-0000-0000-000000000004

```

## `fargo users list`

```text
Description:
  List users as a table, with optional paging and partition filters.
  
  Examples:
    fargo users list
    fargo users list --page 1 --limit 20
    fargo users list --partitions 00000000-0000-0000-0000-000000000001

Usage:
  Fargo.Cli users list [options]

Options:
  --page <page>              Page number to retrieve (starts at 1).
  --limit <limit>            Maximum number of users per page.
  --partitions <partitions>  Only show users under these partition GUIDs (space-separated).
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo users get`

```text
Description:
  Show the full details of a single user.
  
  Examples:
    fargo users get 00000000-0000-0000-0000-000000000004

Usage:
  Fargo.Cli users get <guid> [options]

Arguments:
  <guid>  The user GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo users create`

```text
Description:
  Create a new user account.
  
  Examples:
    fargo users create --username jdoe --password 'S3cr3t!'
    fargo users create --username jdoe --password 'S3cr3t!' --first-name John --last-name Doe

Usage:
  Fargo.Cli users create [options]

Options:
  --username <username> (REQUIRED)  Username (nameid) for the new user.
  --password <password> (REQUIRED)  Initial password for the new user.
  --first-name <first-name>         First name of the user.
  --last-name <last-name>           Last name of the user.
  --active                          Whether the user is active. Defaults to true.
  --username <username>             Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>             Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help                    Show help and usage information
  --server <server>                 The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo users update`

```text
Description:
  Update an existing user. Only the options you pass are changed.
  
  Examples:
    fargo users update 00000000-0000-0000-0000-000000000004 --first-name Jane
    fargo users update 00000000-0000-0000-0000-000000000004 --active false

Usage:
  Fargo.Cli users update <guid> [options]

Arguments:
  <guid>  The user GUID.

Options:
  --username <username>      New username (nameid) for the user.
  --first-name <first-name>  New first name.
  --last-name <last-name>    New last name.
  --active                   Set the user active state (true or false).
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo users delete`

```text
Description:
  Delete a user by GUID.
  
  Examples:
    fargo users delete 00000000-0000-0000-0000-000000000004

Usage:
  Fargo.Cli users delete <guid> [options]

Arguments:
  <guid>  The user GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

