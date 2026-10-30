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
  list                  List service accounts as a table.
  
                        By default the list is paged interactively: a page is shown and you
                        can press 'n' for the next page, 'p' for the previous page, or 'q' to
                        quit (press Enter after the key). Passing --page or --limit switches
                        to a single, non-interactive fetch of exactly that page.
  
                        Examples:
                          fargo service-account list
                          fargo service-account list --page 1 --limit 50
  get <guid>            Show the full details of a single service account. Never shows the secret.
  
                        Examples:
                          fargo service-account get 00000000-0000-0000-0000-000000000010
  create                Create a new service account. The generated client secret is printed
                        EXACTLY ONCE - store it securely, it cannot be retrieved later (only
                        rotated).
  
                        Permissions are numeric ActionType codes (space-separated). Partition
                        accesses are partition GUIDs (space-separated).
  
                        Examples:
                          fargo service-account create --client-id erp-importer
                          fargo service-account create --client-id erp-importer --permissions 1 4 6 --partitions 00000000-0000-0000-0000-000000000002
  update <guid>         Update an existing service account. Only the options you pass are changed.
  
                        Examples:
                          fargo service-account update 00000000-0000-0000-0000-000000000010 --active false
                          fargo service-account update 00000000-0000-0000-0000-000000000010 --add-permissions 4 6 --remove-permissions 2
  rotate-secret <guid>  Rotate a service account's secret. The new secret is printed EXACTLY
                        ONCE and all tokens previously issued to the account stop validating.
  
                        Examples:
                          fargo service-account rotate-secret 00000000-0000-0000-0000-000000000010
  delete <guid>         Delete a service account by GUID. Any tokens it holds stop validating.
  
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
  accesses are partition GUIDs (space-separated).
  
  Examples:
    fargo service-account create --client-id erp-importer
    fargo service-account create --client-id erp-importer --permissions 1 4 6 --partitions 00000000-0000-0000-0000-000000000002

Usage:
  fargo service-account create [options]

Options:
  --client-id <client-id> (REQUIRED)  Unique client identifier (nameid format) for the service account.
  --description <description>         Optional human-readable description.
  --active                            Whether the account is active. Defaults to true.
  --permissions <permissions>         ActionType permission codes to grant (space-separated).
  --partitions <partitions>           Partition GUIDs to grant access to (space-separated).
  --username <username>               Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>               Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help                      Show help and usage information
  --server <server>                   The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose                       Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo service-account update`

```text
Description:
  Update an existing service account. Only the options you pass are changed.
  
  Examples:
    fargo service-account update 00000000-0000-0000-0000-000000000010 --active false
    fargo service-account update 00000000-0000-0000-0000-000000000010 --add-permissions 4 6 --remove-permissions 2

Usage:
  fargo service-account update <guid> [options]

Arguments:
  <guid>  The service account GUID.

Options:
  --description <description>                New description.
  --active                                   Set the active state (true or false).
  --add-permissions <add-permissions>        ActionType permission codes to grant (space-separated).
  --remove-permissions <remove-permissions>  ActionType permission codes to revoke (space-separated).
  --add-partitions <add-partitions>          Partition GUIDs to grant access to (space-separated).
  --remove-partitions <remove-partitions>    Partition GUIDs to revoke access from (space-separated).
  --username <username>                      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>                      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help                             Show help and usage information
  --server <server>                          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose                              Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo service-account rotate-secret`

```text
Description:
  Rotate a service account's secret. The new secret is printed EXACTLY
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

