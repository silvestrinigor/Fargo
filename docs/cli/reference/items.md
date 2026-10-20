# Items

Command reference for `fargo items`, generated from the CLI's help output.

## `fargo items`

```text
Description:
  Manage items - physical instances of articles and their locations.

Usage:
  Fargo.Cli items [command] [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

Commands:
  list              List items as a table, with optional paging and partition filters.
  
                    Examples:
                      fargo items list
                      fargo items list --page 1 --limit 20
                      fargo items list --partitions 00000000-0000-0000-0000-000000000001
  get <guid>        Show the full details of a single item.
  
                    Examples:
                      fargo items get 22222222-2222-2222-2222-222222222222
  create            Create a new item (an instance of an article).
  
                    Examples:
                      fargo items create --article 11111111-1111-1111-1111-111111111111
                      fargo items create --article 11111111-1111-1111-1111-111111111111 --container 22222222-2222-2222-2222-222222222222
  update <guid>     Update an existing item. Only the options you pass are changed.
  
                    Examples:
                      fargo items update 22222222-2222-2222-2222-222222222222 --container 33333333-3333-3333-3333-333333333333
                      fargo items update 22222222-2222-2222-2222-222222222222 --remove-container
  delete <guid>     Delete an item by GUID.
  
                    Examples:
                      fargo items delete 22222222-2222-2222-2222-222222222222
  location <guid>   Show the container chain an item is located in, from the outermost
                    container to the innermost.
  
                    Examples:
                      fargo items location 22222222-2222-2222-2222-222222222222
  moviments <guid>  Show the movement history for an item (when it changed containers).
  
                    Examples:
                      fargo items moviments 22222222-2222-2222-2222-222222222222
  inventory <guid>  Show the article inventory contained inside a container item.
  
                    Examples:
                      fargo items inventory 22222222-2222-2222-2222-222222222222

```

## `fargo items list`

```text
Description:
  List items as a table, with optional paging and partition filters.
  
  Examples:
    fargo items list
    fargo items list --page 1 --limit 20
    fargo items list --partitions 00000000-0000-0000-0000-000000000001

Usage:
  Fargo.Cli items list [options]

Options:
  --page <page>              Page number to retrieve (starts at 1).
  --limit <limit>            Maximum number of items per page.
  --partitions <partitions>  Only show items under these partition GUIDs (space-separated).
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo items get`

```text
Description:
  Show the full details of a single item.
  
  Examples:
    fargo items get 22222222-2222-2222-2222-222222222222

Usage:
  Fargo.Cli items get <guid> [options]

Arguments:
  <guid>  The item GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo items create`

```text
Description:
  Create a new item (an instance of an article).
  
  Examples:
    fargo items create --article 11111111-1111-1111-1111-111111111111
    fargo items create --article 11111111-1111-1111-1111-111111111111 --container 22222222-2222-2222-2222-222222222222

Usage:
  Fargo.Cli items create [options]

Options:
  --article <article>        GUID of the article this item is an instance of.
  --container <container>    GUID of the parent container item.
  --partitions <partitions>  Partition GUIDs to associate (space-separated).
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo items update`

```text
Description:
  Update an existing item. Only the options you pass are changed.
  
  Examples:
    fargo items update 22222222-2222-2222-2222-222222222222 --container 33333333-3333-3333-3333-333333333333
    fargo items update 22222222-2222-2222-2222-222222222222 --remove-container

Usage:
  Fargo.Cli items update <guid> [options]

Arguments:
  <guid>  The item GUID.

Options:
  --container <container>                  New parent container item GUID.
  --remove-container                       Remove the item from its current container.
  --add-partitions <add-partitions>        Partition GUIDs to add (space-separated).
  --remove-partitions <remove-partitions>  Partition GUIDs to remove (space-separated).
  --username <username>                    Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>                    Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help                           Show help and usage information
  --server <server>                        The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo items delete`

```text
Description:
  Delete an item by GUID.
  
  Examples:
    fargo items delete 22222222-2222-2222-2222-222222222222

Usage:
  Fargo.Cli items delete <guid> [options]

Arguments:
  <guid>  The item GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo items location`

```text
Description:
  Show the container chain an item is located in, from the outermost
  container to the innermost.
  
  Examples:
    fargo items location 22222222-2222-2222-2222-222222222222

Usage:
  Fargo.Cli items location <guid> [options]

Arguments:
  <guid>  The item GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo items moviments`

```text
Description:
  Show the movement history for an item (when it changed containers).
  
  Examples:
    fargo items moviments 22222222-2222-2222-2222-222222222222

Usage:
  Fargo.Cli items moviments <guid> [options]

Arguments:
  <guid>  The item GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo items inventory`

```text
Description:
  Show the article inventory contained inside a container item.
  
  Examples:
    fargo items inventory 22222222-2222-2222-2222-222222222222

Usage:
  Fargo.Cli items inventory <guid> [options]

Arguments:
  <guid>  The container item GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

