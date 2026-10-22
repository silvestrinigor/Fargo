# User

Command reference for `fargo user`, generated from the CLI's help output.

## `fargo user`

```text
Description:
  Manage user accounts (list, inspect, create, update, delete).

Usage:
  fargo user [command] [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

Commands:
  list           List users as a table.
  
                 By default the list is paged interactively: a page of users is shown
                 and you can press 'n' for the next page, 'p' for the previous page, or
                 'q' to quit (press Enter after the key). Passing --page or --limit
                 switches to a single, non-interactive fetch of exactly that page.
  
                 Examples:
                   fargo user list
                   fargo user list --page 2
                   fargo user list --page 1 --limit 50
                   fargo user list --partitions 00000000-0000-0000-0000-000000000001
  get <guid>     Show the full details of a single user.
  
                 Examples:
                   fargo user get 00000000-0000-0000-0000-000000000004
  create         Create a new user account.
  
                 Examples:
                   fargo user create --username jdoe --password 'S3cr3t!'
                   fargo user create --username jdoe --password 'S3cr3t!' --first-name John --last-name Doe
  update <guid>  Update an existing user. Only the options you pass are changed.
  
                 Examples:
                   fargo user update 00000000-0000-0000-0000-000000000004 --first-name Jane
                   fargo user update 00000000-0000-0000-0000-000000000004 --active false
  delete <guid>  Delete a user by GUID.
  
                 Examples:
                   fargo user delete 00000000-0000-0000-0000-000000000004

```

## `fargo user list`

```text
Description:
  List users as a table.
  
  By default the list is paged interactively: a page of users is shown
  and you can press 'n' for the next page, 'p' for the previous page, or
  'q' to quit (press Enter after the key). Passing --page or --limit
  switches to a single, non-interactive fetch of exactly that page.
  
  Examples:
    fargo user list
    fargo user list --page 2
    fargo user list --page 1 --limit 50
    fargo user list --partitions 00000000-0000-0000-0000-000000000001

Usage:
  fargo user list [options]

Options:
  --page <page>              Page number to retrieve (starts at 1). Disables interactive paging.
  --limit <limit>            Maximum number of users per page. Disables interactive paging.
  --partitions <partitions>  Only show users under these partition GUIDs (space-separated).
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose              Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user get`

```text
Description:
  Show the full details of a single user.
  
  Examples:
    fargo user get 00000000-0000-0000-0000-000000000004

Usage:
  fargo user get <guid> [options]

Arguments:
  <guid>  The user GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user create`

```text
Description:
  Create a new user account.
  
  Examples:
    fargo user create --username jdoe --password 'S3cr3t!'
    fargo user create --username jdoe --password 'S3cr3t!' --first-name John --last-name Doe

Usage:
  fargo user create [options]

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
  -v, --verbose                     Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user update`

```text
Description:
  Update an existing user. Only the options you pass are changed.
  
  Examples:
    fargo user update 00000000-0000-0000-0000-000000000004 --first-name Jane
    fargo user update 00000000-0000-0000-0000-000000000004 --active false

Usage:
  fargo user update <guid> [options]

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
  -v, --verbose              Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user delete`

```text
Description:
  Delete a user by GUID.
  
  Examples:
    fargo user delete 00000000-0000-0000-0000-000000000004

Usage:
  fargo user delete <guid> [options]

Arguments:
  <guid>  The user GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

