# Service Account

Command reference for `fargo service-account`, generated from the CLI's help output.

## `fargo service-account`

```text
Description:
  Manage service accounts - non-interactive machine identities that authenticate with a client id and secret.

Usage:
  fargo service-account [command] [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

Commands:
  list                        List service accounts as a table.
  
                              By default the list is paged interactively: a page is shown and you
                              can press 'n' for the next page, 'p' for the previous page, or 'q' to
                              quit (press Enter after the key). Passing --page or --limit switches
                              to a single, non-interactive fetch of exactly that page.
  
                              Examples:
                                fargo service-account list
                                fargo service-account list --page 1 --limit 50
  get <guid>                  Show the full details of a single service account. Never shows the secret.
  
                              Examples:
                                fargo service-account get 00000000-0000-0000-0000-000000000010
  create                      Create a new service account. The generated client secret is printed
                              EXACTLY ONCE - store it securely, it cannot be retrieved later (only
                              rotated).
  
                              Permissions are numeric ActionType codes (space-separated). Partition
                              memberships and partition accesses are partition GUIDs (space-separated).
                              Membership defines the partitions the account belongs to (its scope);
                              access defines the partitions the account is allowed to reach.
  
                              Examples:
                                fargo service-account create --client-id erp-importer
                                fargo service-account create --client-id erp-importer --permissions 1 4 6 --partition-memberships 00000000-0000-0000-0000-000000000002 --partition-accesses 00000000-0000-0000-0000-000000000002
  update <guid>               Update the description of an existing service account.
  
                              Activation, permissions and partitions have their own commands: set-active,
                              set-permissions and set-partitions.
  
                              Examples:
                                fargo service-account update 00000000-0000-0000-0000-000000000010 --description "CI runner"
  set-active <guid> <active>  Activate or deactivate a service account.
  
                              This is a separate operation from update and requires its own permission.
  
                              Examples:
                                fargo service-account set-active 00000000-0000-0000-0000-000000000010 true
                                fargo service-account set-active 00000000-0000-0000-0000-000000000010 false
  set-permissions <guid>      Grant and revoke permissions of a service account.
  
                              Permissions are numeric ActionType codes (space-separated). This is a
                              separate operation from update and requires the ChangeServiceAccountPermissions
                              permission. You must hold every permission you grant or revoke.
                              You cannot change your own permissions.
  
                              Examples:
                                fargo service-account set-permissions 00000000-0000-0000-0000-000000000010 --add 4 6
                                fargo service-account set-permissions 00000000-0000-0000-0000-000000000010 --add 4 --remove 2
  set-partitions <guid>       Change the partitions of a service account.
  
                              Membership is the partitions it belongs to; access is the partitions it
                              may reach. This is a separate operation from update and requires the
                              ChangeServiceAccountPartitions permission. You need access to every partition you touch.
  
                              Examples:
                                fargo service-account set-partitions 00000000-0000-0000-0000-000000000010 --add-accesses 00000000-0000-0000-0000-000000000002
                                fargo service-account set-partitions 00000000-0000-0000-0000-000000000010 --add-memberships 00000000-0000-0000-0000-000000000002 --remove-accesses 00000000-0000-0000-0000-000000000005
  rotate-secret <guid>        Rotate a service account's secret. Requires the RotateServiceAccountSecret permission. The new secret is printed EXACTLY
                              ONCE and all tokens previously issued to the account stop validating.
  
                              Examples:
                                fargo service-account rotate-secret 00000000-0000-0000-0000-000000000010
  delete <guid>               Delete a service account by GUID. Any tokens it holds stop validating.
  
                              Examples:
                                fargo service-account delete 00000000-0000-0000-0000-000000000010

```

## `fargo service-account list`

```text
Description:
  List service accounts as a table.
  
  By default the list is paged interactively: a page is shown and you
  can press 'n' for the next page, 'p' for the previous page, or 'q' to
  quit (press Enter after the key). Passing --page or --limit switches
  to a single, non-interactive fetch of exactly that page.
  
  Examples:
    fargo service-account list
    fargo service-account list --page 1 --limit 50

Usage:
  fargo service-account list [options]

Options:
  --page <page>          Page number to retrieve (starts at 1). Disables interactive paging.
  --limit <limit>        Maximum number of service accounts per page. Disables interactive paging.
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo service-account get`

```text
Description:
  Show the full details of a single service account. Never shows the secret.
  
  Examples:
    fargo service-account get 00000000-0000-0000-0000-000000000010

Usage:
  fargo service-account get <guid> [options]

Arguments:
  <guid>  The service account GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo service-account create`

```text
Description:
  Create a new service account. The generated client secret is printed
  EXACTLY ONCE - store it securely, it cannot be retrieved later (only
  rotated).
  
  Permissions are numeric ActionType codes (space-separated). Partition
  memberships and partition accesses are partition GUIDs (space-separated).
  Membership defines the partitions the account belongs to (its scope);
  access defines the partitions the account is allowed to reach.
  
  Examples:
    fargo service-account create --client-id erp-importer
    fargo service-account create --client-id erp-importer --permissions 1 4 6 --partition-memberships 00000000-0000-0000-0000-000000000002 --partition-accesses 00000000-0000-0000-0000-000000000002

Usage:
  fargo service-account create [options]

Options:
  --client-id <client-id> (REQUIRED)                       Unique client identifier (nameid format) for the service account.
  --description <description>                              Optional human-readable description.
  --active                                                 Whether the account is active. Defaults to true.
  --permissions <permissions>                              ActionType permission codes to grant (space-separated). Requires the ChangeServiceAccountPermissions permission.
  --partition-memberships <partition-memberships>          Partition GUIDs the account belongs to - its partition membership (space-separated).
  --partition-accesses, --partitions <partition-accesses>  Partition GUIDs to grant access to (space-separated). Alias: --partitions.
  --username <username>                                    Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>                                    Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help                                           Show help and usage information
  --server <server>                                        The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose                                            Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo service-account update`

```text
Description:
  Update the description of an existing service account.
  
  Activation, permissions and partitions have their own commands: set-active,
  set-permissions and set-partitions.
  
  Examples:
    fargo service-account update 00000000-0000-0000-0000-000000000010 --description "CI runner"

Usage:
  fargo service-account update <guid> [options]

Arguments:
  <guid>  The service account GUID.

Options:
  --description <description>  New description.
  --username <username>        Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>        Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help               Show help and usage information
  --server <server>            The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose                Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo service-account set-active`

```text
Description:
  Activate or deactivate a service account.
  
  This is a separate operation from update and requires its own permission.
  
  Examples:
    fargo service-account set-active 00000000-0000-0000-0000-000000000010 true
    fargo service-account set-active 00000000-0000-0000-0000-000000000010 false

Usage:
  fargo service-account set-active <guid> [<active>] [options]

Arguments:
  <guid>    The service account GUID.
  <active>  true to activate, false to deactivate.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo service-account set-permissions`

```text
Description:
  Grant and revoke permissions of a service account.
  
  Permissions are numeric ActionType codes (space-separated). This is a
  separate operation from update and requires the ChangeServiceAccountPermissions
  permission. You must hold every permission you grant or revoke.
  You cannot change your own permissions.
  
  Examples:
    fargo service-account set-permissions 00000000-0000-0000-0000-000000000010 --add 4 6
    fargo service-account set-permissions 00000000-0000-0000-0000-000000000010 --add 4 --remove 2

Usage:
  fargo service-account set-permissions <guid> [options]

Arguments:
  <guid>  The a service account GUID.

Options:
  --add <add>            ActionType permission codes to grant (space-separated).
  --remove <remove>      ActionType permission codes to revoke (space-separated).
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo service-account set-partitions`

```text
Description:
  Change the partitions of a service account.
  
  Membership is the partitions it belongs to; access is the partitions it
  may reach. This is a separate operation from update and requires the
  ChangeServiceAccountPartitions permission. You need access to every partition you touch.
  
  Examples:
    fargo service-account set-partitions 00000000-0000-0000-0000-000000000010 --add-accesses 00000000-0000-0000-0000-000000000002
    fargo service-account set-partitions 00000000-0000-0000-0000-000000000010 --add-memberships 00000000-0000-0000-0000-000000000002 --remove-accesses 00000000-0000-0000-0000-000000000005

Usage:
  fargo service-account set-partitions <guid> [options]

Arguments:
  <guid>  The a service account GUID.

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

## `fargo service-account rotate-secret`

```text
Description:
  Rotate a service account's secret. Requires the RotateServiceAccountSecret permission. The new secret is printed EXACTLY
  ONCE and all tokens previously issued to the account stop validating.
  
  Examples:
    fargo service-account rotate-secret 00000000-0000-0000-0000-000000000010

Usage:
  fargo service-account rotate-secret <guid> [options]

Arguments:
  <guid>  The service account GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo service-account delete`

```text
Description:
  Delete a service account by GUID. Any tokens it holds stop validating.
  
  Examples:
    fargo service-account delete 00000000-0000-0000-0000-000000000010

Usage:
  fargo service-account delete <guid> [options]

Arguments:
  <guid>  The service account GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

