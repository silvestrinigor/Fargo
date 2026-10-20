# Articles

Command reference for `fargo articles`, generated from the CLI's help output.

## `fargo articles`

```text
Description:
  Manage articles - the catalog of product definitions.

Usage:
  Fargo.Cli articles [command] [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

Commands:
  list                      List articles as a table, with optional paging and partition filters.
  
                            Examples:
                              fargo articles list
                              fargo articles list --page 1 --limit 20
                              fargo articles list --partitions 00000000-0000-0000-0000-000000000001
  get <guid>                Show the full details of a single article.
  
                            Examples:
                              fargo articles get 11111111-1111-1111-1111-111111111111
  get-by-barcode <barcode>  Look up a single article by one of its barcodes.
  
                            The barcode is given as {value}:{type}.
  
                            Examples:
                              fargo articles get-by-barcode 1234567890128:ean13
  inventory <guid>          Show the inventory count for an article.
  
                            Examples:
                              fargo articles inventory 11111111-1111-1111-1111-111111111111
                              fargo articles inventory 11111111-1111-1111-1111-111111111111 --include-descendants false
  create                    Create a new article.
  
                            Examples:
                              fargo articles create --name "Widget"
                              fargo articles create --name "Widget" --type pack --partitions 00000000-0000-0000-0000-000000000001
  delete <guid>             Delete an article by GUID.
  
                            Examples:
                              fargo articles delete 11111111-1111-1111-1111-111111111111
  update <guid>             Update an existing article. Only the options you pass are changed.
  
                            Examples:
                              fargo articles update 11111111-1111-1111-1111-111111111111 --name "New name"
                              fargo articles update 11111111-1111-1111-1111-111111111111 --add-partitions 00000000-0000-0000-0000-000000000001

```

## `fargo articles list`

```text
Description:
  List articles as a table, with optional paging and partition filters.
  
  Examples:
    fargo articles list
    fargo articles list --page 1 --limit 20
    fargo articles list --partitions 00000000-0000-0000-0000-000000000001

Usage:
  Fargo.Cli articles list [options]

Options:
  --page <page>              Page number to retrieve (starts at 1).
  --limit <limit>            Maximum number of articles per page.
  --partitions <partitions>  Only show articles under these partition GUIDs (space-separated).
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo articles get`

```text
Description:
  Show the full details of a single article.
  
  Examples:
    fargo articles get 11111111-1111-1111-1111-111111111111

Usage:
  Fargo.Cli articles get <guid> [options]

Arguments:
  <guid>  The article GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo articles get-by-barcode`

```text
Description:
  Look up a single article by one of its barcodes.
  
  The barcode is given as {value}:{type}.
  
  Examples:
    fargo articles get-by-barcode 1234567890128:ean13

Usage:
  Fargo.Cli articles get-by-barcode <barcode> [options]

Arguments:
  <barcode>  The barcode in {value}:{type} format (e.g. 1234567890128:ean13).

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo articles inventory`

```text
Description:
  Show the inventory count for an article.
  
  Examples:
    fargo articles inventory 11111111-1111-1111-1111-111111111111
    fargo articles inventory 11111111-1111-1111-1111-111111111111 --include-descendants false

Usage:
  Fargo.Cli articles inventory <guid> [options]

Arguments:
  <guid>  The article GUID.

Options:
  --containers <containers>  Limit inventory to these container item GUIDs (space-separated).
  --include-descendants      Include items in nested containers. Defaults to true.
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo articles create`

```text
Description:
  Create a new article.
  
  Examples:
    fargo articles create --name "Widget"
    fargo articles create --name "Widget" --type pack --partitions 00000000-0000-0000-0000-000000000001

Usage:
  Fargo.Cli articles create [options]

Options:
  --name <name> (REQUIRED)                       Name of the article.
  --type <Container|Default|Kit|Pack|Variation>  Article type by name (default, variation, pack, kit, container). Defaults to default. [default: Default]
  --partitions <partitions>                      Partition GUIDs to associate the article with (space-separated).
  --username <username>                          Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>                          Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help                                 Show help and usage information
  --server <server>                              The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo articles update`

```text
Description:
  Update an existing article. Only the options you pass are changed.
  
  Examples:
    fargo articles update 11111111-1111-1111-1111-111111111111 --name "New name"
    fargo articles update 11111111-1111-1111-1111-111111111111 --add-partitions 00000000-0000-0000-0000-000000000001

Usage:
  Fargo.Cli articles update <guid> [options]

Arguments:
  <guid>  The article GUID.

Options:
  --name <name>                            New name for the article.
  --add-partitions <add-partitions>        Partition GUIDs to add (space-separated).
  --remove-partitions <remove-partitions>  Partition GUIDs to remove (space-separated).
  --username <username>                    Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>                    Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help                           Show help and usage information
  --server <server>                        The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

## `fargo articles delete`

```text
Description:
  Delete an article by GUID.
  
  Examples:
    fargo articles delete 11111111-1111-1111-1111-111111111111

Usage:
  Fargo.Cli articles delete <guid> [options]

Arguments:
  <guid>  The article GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.

```

