# Item

Command reference for `fargo item`, generated from the CLI's help output.

## `fargo item`

```text
Description:
  Manage items - physical instances of articles and their locations.

Usage:
  fargo item [command] [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

Commands:
  list              List items as a table.
  
                    By default the list is paged interactively: a page of items is shown
                    and you can press 'n' for the next page, 'p' for the previous page, or
                    'q' to quit (press Enter after the key). Passing --page or --limit
                    switches to a single, non-interactive fetch of exactly that page.
  
                    Examples:
                      fargo item list
                      fargo item list --page 2
                      fargo item list --page 1 --limit 50
                      fargo item list --partitions 00000000-0000-0000-0000-000000000001
  get <guid>        Show the full details of a single item.
  
                    Examples:
                      fargo item get 22222222-2222-2222-2222-222222222222
  create            Create a new item (an instance of an article).
  
                    Examples:
                      fargo item create --article 11111111-1111-1111-1111-111111111111
                      fargo item create --article 11111111-1111-1111-1111-111111111111 --container 22222222-2222-2222-2222-222222222222
  update <guid>     Update an existing item. Only the options you pass are changed.
  
                    Examples:
                      fargo item update 22222222-2222-2222-2222-222222222222 --container 33333333-3333-3333-3333-333333333333
                      fargo item update 22222222-2222-2222-2222-222222222222 --remove-container
  move              Record a movement of a quantity of an event-tracked article between
                    containers.
  
                    Only valid for articles whose tracking mode is Event. Provide a
                    destination (--to) for a receipt, an origin (--from) for a dispatch,
                    or both for a transfer.
  
                    Examples:
                      fargo item move --article 11111111-1111-1111-1111-111111111111 --quantity 50 --to 22222222-2222-2222-2222-222222222222
                      fargo item move --article 11111111-1111-1111-1111-111111111111 --quantity 10 --from 22222222-2222-2222-2222-222222222222
  delete <guid>     Delete an item by GUID.
  
                    Examples:
                      fargo item delete 22222222-2222-2222-2222-222222222222
  location <guid>   Show where an item is located as a nested container tree, from the
                    outermost container inward to the item itself.
  
                    Pass --at to reconstruct the location as it was at a specific
                    instant, using the item's containment history, instead of the
                    current location.
  
                    Examples:
                      fargo item location 22222222-2222-2222-2222-222222222222
                      fargo item location 22222222-2222-2222-2222-222222222222 --at 2026-03-01T12:00:00Z
  moviments <guid>  Show the movement history for an item (when it changed containers).
  
                    Examples:
                      fargo item moviments 22222222-2222-2222-2222-222222222222
  inventory <guid>  Show the article inventory contained inside a container item.
  
                    Pass --at to reconstruct the inventory as it was at a specific
                    instant, using the item containment history, instead of the current
                    inventory.
  
                    Examples:
                      fargo item inventory 22222222-2222-2222-2222-222222222222
                      fargo item inventory 22222222-2222-2222-2222-222222222222 --at 2026-03-01T12:00:00Z

```

## `fargo item list`

```text
Description:
  List items as a table.
  
  By default the list is paged interactively: a page of items is shown
  and you can press 'n' for the next page, 'p' for the previous page, or
  'q' to quit (press Enter after the key). Passing --page or --limit
  switches to a single, non-interactive fetch of exactly that page.
  
  Examples:
    fargo item list
    fargo item list --page 2
    fargo item list --page 1 --limit 50
    fargo item list --partitions 00000000-0000-0000-0000-000000000001

Usage:
  fargo item list [options]

Options:
  --page <page>              Page number to retrieve (starts at 1). Disables interactive paging.
  --limit <limit>            Maximum number of items per page. Disables interactive paging.
  --partitions <partitions>  Only show items under these partition GUIDs (space-separated).
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose              Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo item get`

```text
Description:
  Show the full details of a single item.
  
  Examples:
    fargo item get 22222222-2222-2222-2222-222222222222

Usage:
  fargo item get <guid> [options]

Arguments:
  <guid>  The item GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo item create`

```text
Description:
  Create a new item (an instance of an article).
  
  Examples:
    fargo item create --article 11111111-1111-1111-1111-111111111111
    fargo item create --article 11111111-1111-1111-1111-111111111111 --container 22222222-2222-2222-2222-222222222222

Usage:
  fargo item create [options]

Options:
  --article <article>        GUID of the article this item is an instance of.
  --container <container>    GUID of the parent container item.
  --partitions <partitions>  Partition GUIDs to associate (space-separated).
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose              Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo item update`

```text
Description:
  Update an existing item. Only the options you pass are changed.
  
  Examples:
    fargo item update 22222222-2222-2222-2222-222222222222 --container 33333333-3333-3333-3333-333333333333
    fargo item update 22222222-2222-2222-2222-222222222222 --remove-container

Usage:
  fargo item update <guid> [options]

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
  -v, --verbose                            Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo item move`

```text
Description:
  Record a movement of a quantity of an event-tracked article between containers.
  
  Only valid for articles whose tracking mode is Event. Provide a destination
  (--to) for a receipt, an origin (--from) for a dispatch, or both for a transfer.
  
  Examples:
    # Receive 50 units into a container
    fargo item move --article 11111111-1111-1111-1111-111111111111 --quantity 50 --to 22222222-2222-2222-2222-222222222222
  
    # Dispatch 10 units out of a container
    fargo item move --article 11111111-1111-1111-1111-111111111111 --quantity 10 --from 22222222-2222-2222-2222-222222222222
  
    # Transfer 5 units between containers
    fargo item move --article 11111111-1111-1111-1111-111111111111 --quantity 5 --from 22222222-2222-2222-2222-222222222222 --to 33333333-3333-3333-3333-333333333333

Usage:
  fargo item move [options]

Options:
  --article <article>    GUID of the event-tracked article being moved.
  --quantity <quantity>  The strictly positive quantity moved.
  --from <from>          GUID of the origin container. Omit for a receipt from outside any container.
  --to <to>              GUID of the destination container. Omit for a dispatch to outside any container.
  --at <at>              Instant (ISO 8601 UTC) at which the movement occurred. Defaults to now.
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo item delete`

```text
Description:
  Delete an item by GUID.
  
  Examples:
    fargo item delete 22222222-2222-2222-2222-222222222222

Usage:
  fargo item delete <guid> [options]

Arguments:
  <guid>  The item GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo item location`

```text
Description:
  Show where an item is located as a nested container tree, from the
  outermost container inward to the item itself.
  
  Pass --at to reconstruct the location as it was at a specific
  instant, using the item's containment history, instead of the
  current location.
  
  Examples:
    fargo item location 22222222-2222-2222-2222-222222222222
    fargo item location 22222222-2222-2222-2222-222222222222 --at 2026-03-01T12:00:00Z

Usage:
  fargo item location <guid> [options]

Arguments:
  <guid>  The item GUID.

Options:
  --at <at>              Instant (ISO 8601, e.g. 2026-03-01T12:00:00Z) at which to evaluate the location. Defaults to now.
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo item moviments`

```text
Description:
  Show the movement history for an item (when it changed containers).
  
  Examples:
    fargo item moviments 22222222-2222-2222-2222-222222222222

Usage:
  fargo item moviments <guid> [options]

Arguments:
  <guid>  The item GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo item inventory`

```text
Description:
  Show the article inventory contained inside a container item.
  
  Pass --at to reconstruct the inventory as it was at a specific
  instant, using the item containment history, instead of the current
  inventory.
  
  Examples:
    fargo item inventory 22222222-2222-2222-2222-222222222222
    fargo item inventory 22222222-2222-2222-2222-222222222222 --at 2026-03-01T12:00:00Z

Usage:
  fargo item inventory <guid> [options]

Arguments:
  <guid>  The container item GUID.

Options:
  --at <at>              Instant (ISO 8601, e.g. 2026-03-01T12:00:00Z) at which to evaluate the inventory. Defaults to now.
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

