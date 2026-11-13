# User

Command reference for `fargo user`, generated from the CLI's help output.

## `fargo user`

```text
Description:
  Manage user accounts (list, inspect, create, update, activate or deactivate, delete).

Usage:
  fargo user [command] [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

Commands:
  list                                     List users as a table.
  
                                           By default the list is paged interactively: a page of users is shown
                                           and you can press 'n' for the next page, 'p' for the previous page, or
                                           'q' to quit (press Enter after the key). Passing --page or --limit
                                           switches to a single, non-interactive fetch of exactly that page.
  
                                           Examples:
                                             fargo user list
                                             fargo user list --page 2
                                             fargo user list --page 1 --limit 50
                                             fargo user list --partitions 00000000-0000-0000-0000-000000000001
  get <guid>                               Show the full details of a single user.
  
                                           Examples:
                                             fargo user get 00000000-0000-0000-0000-000000000004
  create                                   Create a new user account.
  
                                           Examples:
                                             fargo user create --username jdoe --password 'S3cr3t!'
                                             fargo user create --username jdoe --password 'S3cr3t!' --first-name John --last-name Doe
  update <guid>                            Update an existing user. Only the options you pass are changed.
  
                                           Examples:
                                             fargo user update 00000000-0000-0000-0000-000000000004 --first-name Jane
  set-active <guid> <active>               Activate or deactivate a user.
  
                                           This is a separate operation from update and requires its own permission.
  
                                           Examples:
                                             fargo user set-active 00000000-0000-0000-0000-000000000004 true
                                             fargo user set-active 00000000-0000-0000-0000-000000000004 false
  set-permissions <guid>                   Grant and revoke permissions of a user.
  
                                           Permissions are numeric ActionType codes (space-separated). This is a
                                           separate operation from update and requires the ChangeUserPermissions
                                           permission. You must hold every permission you grant or revoke.
                                           You cannot change your own permissions.
  
                                           Examples:
                                             fargo user set-permissions 00000000-0000-0000-0000-000000000004 --add 4 6
                                             fargo user set-permissions 00000000-0000-0000-0000-000000000004 --add 4 --remove 2
  set-groups <guid>                        Add a user to, and remove it from, user groups.
  
                                           Joining a group hands the user the permissions of that group and of its
                                           ancestors, so you must hold all of them. This is a separate operation
                                           from update and requires the ChangeUserGroupMembers permission.
  
                                           Examples:
                                             fargo user set-groups 00000000-0000-0000-0000-000000000004 --add 00000000-0000-0000-0000-000000000003
                                             fargo user set-groups 00000000-0000-0000-0000-000000000004 --remove 00000000-0000-0000-0000-000000000003
  set-partitions <guid>                    Change the partitions of a user.
  
                                           Membership is the partitions it belongs to; access is the partitions it
                                           may reach. This is a separate operation from update and requires the
                                           ChangeUserPartitions permission. You need access to every partition you touch.
  
                                           Examples:
                                             fargo user set-partitions 00000000-0000-0000-0000-000000000004 --add-accesses 00000000-0000-0000-0000-000000000002
                                             fargo user set-partitions 00000000-0000-0000-0000-000000000004 --add-memberships 00000000-0000-0000-0000-000000000002 --remove-accesses 00000000-0000-0000-0000-000000000005
  reset-password <guid>                    Reset the password of a user.
  
                                           The user must choose a new password at the next sign-in. You cannot reset
                                           your own password this way; use 'identity change-password' instead.
                                           Requires the ChangeAnotherUserPassword permission.
  
                                           Examples:
                                             fargo user reset-password 00000000-0000-0000-0000-000000000004 --password 'Temp-S3cr3t!'
  set-password-expiration <guid> <period>  Set how long the passwords of a user last by default.
  
                                           The period is an ISO-8601 duration. It applies the next time the password is
                                           changed; PT0S means it must be changed immediately. Requires the
                                           ChangeAnotherUserPassword permission.
  
                                           Examples:
                                             fargo user set-password-expiration 00000000-0000-0000-0000-000000000004 P90D
  remove-password-expiration <guid>        Stop the passwords of a user from expiring by default.
  
                                           Requires the ChangeAnotherUserPassword permission.
  
                                           Examples:
                                             fargo user remove-password-expiration 00000000-0000-0000-0000-000000000004
  delete <guid>                            Delete a user by GUID.
  
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
  --sort <sort>              Sort field, optionally prefixed with '-' for descending (e.g. -name). Fields: guid, nameid, firstname, lastname, isactive.
  --search <search>          Case-insensitive text to match against the user nameid.
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

Usage:
  fargo user update <guid> [options]

Arguments:
  <guid>  The user GUID.

Options:
  --username <username>      New username (nameid) for the user.
  --first-name <first-name>  New first name.
  --last-name <last-name>    New last name.
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose              Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user set-active`

```text
Description:
  Activate or deactivate a user.
  
  This is a separate operation from update and requires its own permission.
  
  Examples:
    fargo user set-active 00000000-0000-0000-0000-000000000004 true
    fargo user set-active 00000000-0000-0000-0000-000000000004 false

Usage:
  fargo user set-active <guid> [<active>] [options]

Arguments:
  <guid>    The user GUID.
  <active>  true to activate, false to deactivate.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user set-permissions`

```text
Description:
  Grant and revoke permissions of a user.
  
  Permissions are numeric ActionType codes (space-separated). This is a
  separate operation from update and requires the ChangeUserPermissions
  permission. You must hold every permission you grant or revoke.
  You cannot change your own permissions.
  
  Examples:
    fargo user set-permissions 00000000-0000-0000-0000-000000000004 --add 4 6
    fargo user set-permissions 00000000-0000-0000-0000-000000000004 --add 4 --remove 2

Usage:
  fargo user set-permissions <guid> [options]

Arguments:
  <guid>  The a user GUID.

Options:
  --add <add>            ActionType permission codes to grant (space-separated).
  --remove <remove>      ActionType permission codes to revoke (space-separated).
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user set-groups`

```text
Description:
  Add a user to, and remove it from, user groups.
  
  Joining a group hands the user the permissions of that group and of its
  ancestors, so you must hold all of them. This is a separate operation
  from update and requires the ChangeUserGroupMembers permission.
  
  Examples:
    fargo user set-groups 00000000-0000-0000-0000-000000000004 --add 00000000-0000-0000-0000-000000000003
    fargo user set-groups 00000000-0000-0000-0000-000000000004 --remove 00000000-0000-0000-0000-000000000003

Usage:
  fargo user set-groups <guid> [options]

Arguments:
  <guid>  The user GUID.

Options:
  --add <add>            User group GUIDs to add the user to (space-separated).
  --remove <remove>      User group GUIDs to remove the user from (space-separated).
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user set-partitions`

```text
Description:
  Change the partitions of a user.
  
  Membership is the partitions it belongs to; access is the partitions it
  may reach. This is a separate operation from update and requires the
  ChangeUserPartitions permission. You need access to every partition you touch.
  
  Examples:
    fargo user set-partitions 00000000-0000-0000-0000-000000000004 --add-accesses 00000000-0000-0000-0000-000000000002
    fargo user set-partitions 00000000-0000-0000-0000-000000000004 --add-memberships 00000000-0000-0000-0000-000000000002 --remove-accesses 00000000-0000-0000-0000-000000000005

Usage:
  fargo user set-partitions <guid> [options]

Arguments:
  <guid>  The a user GUID.

Options:
  --add-memberships <add-memberships>        Partition GUIDs to add to the membership (space-separated).
  --remove-memberships <remove-memberships>  Partition GUIDs to remove from the membership (space-separated).
  --add-accesses <add-accesses>              Partition GUIDs to grant access to (space-separated).
  --remove-accesses <remove-accesses>        Partition GUIDs to revoke access from (space-separated).
  --username <username>                      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>                      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help                             Show help and usage information
  --server <server>                          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose                              Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user reset-password`

```text
Description:
  Reset the password of a user.
  
  The user must choose a new password at the next sign-in. You cannot reset
  your own password this way; use 'identity change-password' instead.
  Requires the ChangeAnotherUserPassword permission.
  
  Examples:
    fargo user reset-password 00000000-0000-0000-0000-000000000004 --password 'Temp-S3cr3t!'

Usage:
  fargo user reset-password <guid> [options]

Arguments:
  <guid>  The user GUID.

Options:
  --password <password> (REQUIRED)  The new temporary password.
  --username <username>             Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>             Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help                    Show help and usage information
  --server <server>                 The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose                     Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user set-password-expiration`

```text
Description:
  Set how long the passwords of a user last by default.
  
  The period is an ISO-8601 duration. It applies the next time the password is
  changed; PT0S means it must be changed immediately. Requires the
  ChangeAnotherUserPassword permission.
  
  Examples:
    fargo user set-password-expiration 00000000-0000-0000-0000-000000000004 P90D

Usage:
  fargo user set-password-expiration <guid> <period> [options]

Arguments:
  <guid>    The user GUID.
  <period>  The expiration period as an ISO-8601 duration, e.g. P90D.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo user remove-password-expiration`

```text
Description:
  Stop the passwords of a user from expiring by default.
  
  Requires the ChangeAnotherUserPassword permission.
  
  Examples:
    fargo user remove-password-expiration 00000000-0000-0000-0000-000000000004

Usage:
  fargo user remove-password-expiration <guid> [options]

Arguments:
  <guid>  The user GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

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

