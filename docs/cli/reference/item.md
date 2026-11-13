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
  list                          List items as a table.
  
                                By default the list is paged interactively: a page of items is shown
                                and you can press 'n' for the next page, 'p' for the previous page, or
                                'q' to quit (press Enter after the key). Passing --page or --limit
                                switches to a single, non-interactive fetch of exactly that page.
  
                                Examples:
                                  fargo item list
                                  fargo item list --page 2
                                  fargo item list --page 1 --limit 50
                                  fargo item list --partitions 00000000-0000-0000-0000-000000000001
  get <guid>                    Show the full details of a single item.
  
                                Examples:
                                  fargo item get 22222222-2222-2222-2222-222222222222
  create                        Create a new item (an instance of an article).
  
                                Examples:
                                  fargo item create --article 11111111-1111-1111-1111-111111111111
                                  fargo item create --article 11111111-1111-1111-1111-111111111111 --container 22222222-2222-2222-2222-222222222222
                                  fargo item create --article 11111111-1111-1111-1111-111111111111 --idempotency-key 3f2b8c1e-9d4a-4b7e-8c2f-1a5d6e7f8091
  set-partitions <guid>         Change the partitions of an item.
  
                                This is a separate operation from update and requires the ChangeItemPartitions
                                permission. You need access to every partition you touch.
  
                                Examples:
                                  fargo item set-partitions 22222222-2222-2222-2222-222222222222 --add 00000000-0000-0000-0000-000000000002
                                  fargo item set-partitions 22222222-2222-2222-2222-222222222222 --add 00000000-0000-0000-0000-000000000002 --remove 00000000-0000-0000-0000-000000000005
  move <guid>                   Place an item inside a container.
  
                                This is a separate operation from update and requires its own permission.
                                To move stock of an event-tracked article, use 'item event move' instead.
  
                                Examples:
                                  fargo item move 22222222-2222-2222-2222-222222222222 --to 33333333-3333-3333-3333-333333333333
  remove-from-container <guid>  Remove an item from its current container.
  
                                This is a separate operation from update and requires its own permission.
  
                                Examples:
                                  fargo item remove-from-container 22222222-2222-2222-2222-222222222222
  delete <guid>                 Delete an item by GUID.
  
                                Examples:
                                  fargo item delete 22222222-2222-2222-2222-222222222222
  location <guid>               Show where an item is located as a nested container tree, from the
                                outermost container inward to the item itself.
  
                                Pass --at to reconstruct the location as it was at a specific
                                instant, using the item's containment history, instead of the
                                current location.
  
                                Examples:
                                  fargo item location 22222222-2222-2222-2222-222222222222
                                  fargo item location 22222222-2222-2222-2222-222222222222 --at 2026-03-01T12:00:00Z
  moviments <guid>              Show the movement history for an item (when it changed containers).
  
                                Examples:
                                  fargo item moviments 22222222-2222-2222-2222-222222222222
  inventory <guid>              Show the article inventory contained inside a container item.
  
                                Pass --at to reconstruct the inventory as it was at a specific
                                instant, using the item containment history, instead of the current
                                inventory.
  
                                Examples:
                                  fargo item inventory 22222222-2222-2222-2222-222222222222
                                  fargo item inventory 22222222-2222-2222-2222-222222222222 --at 2026-03-01T12:00:00Z
  event                         Record and query item events - stock changes for event-tracked articles.

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
  --sort <sort>              Sort field, optionally prefixed with '-' for descending (e.g. -name). Fields: guid, article, parentcontainer, isfixed.
  --search <search>          Case-insensitive text to match against the article barcode of the item.
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
    fargo item create --article 11111111-1111-1111-1111-111111111111 --idempotency-key 3f2b8c1e-9d4a-4b7e-8c2f-1a5d6e7f8091

Usage:
  fargo item create [options]

Options:
  --article <article>                  GUID of the article this item is an instance of.
  --container <container>              GUID of the parent container item.
  --partitions <partitions>            Partition GUIDs to associate (space-separated).
  --idempotency-key <idempotency-key>  A GUID that makes the request safe to retry: sending the same key again returns the first result instead of repeating it. Use a new key for a different request. A new key is generated when omitted.
  --username <username>                Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>                Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help                       Show help and usage information
  --server <server>                    The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose                        Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo item set-partitions`

```text
Description:
  Change the partitions of an item.
  
  This is a separate operation from update and requires the ChangeItemPartitions
  permission. You need access to every partition you touch.
  
  Examples:
    fargo item set-partitions 22222222-2222-2222-2222-222222222222 --add 00000000-0000-0000-0000-000000000002
    fargo item set-partitions 22222222-2222-2222-2222-222222222222 --add 00000000-0000-0000-0000-000000000002 --remove 00000000-0000-0000-0000-000000000005

Usage:
  fargo item set-partitions <guid> [options]

Arguments:
  <guid>  The an item GUID.

Options:
  --add <add>            Partition GUIDs to add (space-separated).
  --remove <remove>      Partition GUIDs to remove (space-separated).
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo item move`

```text
Description:
  Place an item inside a container.
  
  This is a separate operation from update and requires its own permission.
  To move stock of an event-tracked article, use 'item event move' instead.
  
  Examples:
    fargo item move 22222222-2222-2222-2222-222222222222 --to 33333333-3333-3333-3333-333333333333

Usage:
  fargo item move <guid> [options]

Arguments:
  <guid>  The item GUID.

Options:
  --to <to> (REQUIRED)   GUID of the destination container item.
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo item remove-from-container`

```text
Description:
  Remove an item from its current container.
  
  This is a separate operation from update and requires its own permission.
  
  Examples:
    fargo item remove-from-container 22222222-2222-2222-2222-222222222222

Usage:
  fargo item remove-from-container <guid> [options]

Arguments:
  <guid>  The item GUID.

Options:
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

