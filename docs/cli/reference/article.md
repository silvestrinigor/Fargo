# Article

Command reference for `fargo article`, generated from the CLI's help output.

## `fargo article`

```text
Description:
  Manage articles - the catalog of product definitions.

Usage:
  fargo article [command] [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

Commands:
  list                         List articles as a table, with optional paging and partition filters.
  
                               Examples:
                                 fargo article list
                                 fargo article list --page 1 --limit 20
                                 fargo article list --partitions 00000000-0000-0000-0000-000000000001
  get <guid>                   Show the full details of a single article.
  
                               Examples:
                                 fargo article get 11111111-1111-1111-1111-111111111111
  get-by-barcode <barcode>     Look up a single article by one of its barcodes.
  
                               The barcode is given as {value}:{type}.
  
                               Examples:
                                 fargo article get-by-barcode 1234567890128:ean13
  inventory <guid>             Show the inventory count for an article.
  
                               Examples:
                                 fargo article inventory 11111111-1111-1111-1111-111111111111
                                 fargo article inventory 11111111-1111-1111-1111-111111111111 --include-descendants false
  create                       Create a new article.
  
                               The article type is inferred from the options you pass; you do not set
                               it explicitly:
                                 - variation : --variation-from <guid>
                                 - pack      : --pack-from <guid> and/or --pack-quantity <n>
                                 - kit       : one or more --kit-component
                                 - container : --is-container
                                 - default   : none of the above
                               These are mutually exclusive: combining options that imply different
                               types (for example --variation-from with --pack-from) is an error.
  
                               Measures are written as a number followed by a unit, e.g. '2.5kg' or
                               '10cm'. Units are standard abbreviations (mass: g, kg, mg, lb, oz;
                               length: mm, cm, m, in, ft). Kit components are written as
                               '<article-guid>' or '<article-guid>:<quantity>' and may be repeated.
  
                               Examples:
                                 fargo article create --name "Widget"
                                 fargo article create --name "Widget" --mass 2.5kg --length-x 10cm --length-y 5cm --length-z 2cm
                                 fargo article create --name "Bundle" \
                                   --kit-component 11111111-1111-1111-1111-111111111111:2 \
                                   --kit-component 22222222-2222-2222-2222-222222222222
                                 fargo article create --name "Case" --is-container --container-max-mass 25kg
                                 fargo article create --name "6-pack" \
                                   --pack-from 11111111-1111-1111-1111-111111111111 --pack-quantity 6
                                 fargo article create --name "Blue Widget" \
                                   --variation-from 11111111-1111-1111-1111-111111111111
                                 fargo article create --name "Widget" --barcode-ean13 1234567890128
  delete <guid>                Delete an article by GUID.
  
                               Examples:
                                 fargo article delete 11111111-1111-1111-1111-111111111111
  delete-by-barcode <barcode>  Delete an article addressed by one of its barcodes.
  
                               The barcode is given as {value}:{type} (e.g. 1234567890128:ean13).
  
                               Examples:
                                 fargo article delete-by-barcode 1234567890128:ean13
  update <guid>                Update an existing article by GUID. Only the options you pass are changed.
  
                               Measures are written as a number followed by a unit, e.g. '2.5kg' or
                               '10cm'. Use the matching --remove-* flag to clear a value.
  
                               Examples:
                                 fargo article update 11111111-1111-1111-1111-111111111111 --name "New name"
                                 fargo article update 11111111-1111-1111-1111-111111111111 --mass 3kg --length-x 12cm
                                 fargo article update 11111111-1111-1111-1111-111111111111 --remove-mass --remove-barcode
                                 fargo article update 11111111-1111-1111-1111-111111111111 --barcode-ean13 1234567890128
                                 fargo article update 11111111-1111-1111-1111-111111111111 --add-partitions 00000000-0000-0000-0000-000000000001
  update-by-barcode <barcode>  Update an existing article addressed by one of its barcodes. Only the
                               options you pass are changed.
  
                               The barcode is given as {value}:{type} (e.g. 1234567890128:ean13).
                               Measures are written as a number followed by a unit, e.g. '2.5kg'.
  
                               Examples:
                                 fargo article update-by-barcode 1234567890128:ean13 --name "New name"
                                 fargo article update-by-barcode 1234567890128:ean13 --mass 3kg --remove-shelf-life

```

## `fargo article list`

```text
Description:
  List articles as a table, with optional paging and partition filters.
  
  Examples:
    fargo article list
    fargo article list --page 1 --limit 20
    fargo article list --partitions 00000000-0000-0000-0000-000000000001

Usage:
  fargo article list [options]

Options:
  --page <page>              Page number to retrieve (starts at 1).
  --limit <limit>            Maximum number of articles per page.
  --partitions <partitions>  Only show articles under these partition GUIDs (space-separated).
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose              Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo article get`

```text
Description:
  Show the full details of a single article.
  
  Examples:
    fargo article get 11111111-1111-1111-1111-111111111111

Usage:
  fargo article get <guid> [options]

Arguments:
  <guid>  The article GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo article get-by-barcode`

```text
Description:
  Look up a single article by one of its barcodes.
  
  The barcode is given as {value}:{type}.
  
  Examples:
    fargo article get-by-barcode 1234567890128:ean13

Usage:
  fargo article get-by-barcode <barcode> [options]

Arguments:
  <barcode>  The barcode in {value}:{type} format (e.g. 1234567890128:ean13).

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo article inventory`

```text
Description:
  Show the inventory count for an article.
  
  Examples:
    fargo article inventory 11111111-1111-1111-1111-111111111111
    fargo article inventory 11111111-1111-1111-1111-111111111111 --include-descendants false

Usage:
  fargo article inventory <guid> [options]

Arguments:
  <guid>  The article GUID.

Options:
  --containers <containers>  Limit inventory to these container item GUIDs (space-separated).
  --include-descendants      Include items in nested containers. Defaults to true.
  --username <username>      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help             Show help and usage information
  --server <server>          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose              Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo article create`

```text
Description:
  Create a new article.
  
  The article type is inferred from the options you pass; you do not set
  it explicitly:
    - variation : --variation-from <guid>
    - pack      : --pack-from <guid> and/or --pack-quantity <n>
    - kit       : one or more --kit-component
    - container : --is-container
    - default   : none of the above
  These are mutually exclusive: combining options that imply different
  types (for example --variation-from with --pack-from) is an error.
  
  Measures are written as a number followed by a unit, e.g. '2.5kg' or
  '10cm'. Units are standard abbreviations (mass: g, kg, mg, lb, oz;
  length: mm, cm, m, in, ft). Kit components are written as
  '<article-guid>' or '<article-guid>:<quantity>' and may be repeated.
  
  Examples:
    fargo article create --name "Widget"
    fargo article create --name "Widget" --mass 2.5kg --length-x 10cm --length-y 5cm --length-z 2cm
    fargo article create --name "Bundle" \
      --kit-component 11111111-1111-1111-1111-111111111111:2 \
      --kit-component 22222222-2222-2222-2222-222222222222
    fargo article create --name "Case" --is-container --container-max-mass 25kg
    fargo article create --name "6-pack" \
      --pack-from 11111111-1111-1111-1111-111111111111 --pack-quantity 6
    fargo article create --name "Blue Widget" \
      --variation-from 11111111-1111-1111-1111-111111111111
    fargo article create --name "Widget" --barcode-ean13 1234567890128

Usage:
  fargo article create [options]

Options:
  --name <name> (REQUIRED)                   Name of the article.
  --description <description>                Optional description of the article.
  --shelf-life <shelf-life>                  Optional shelf life (e.g. an ISO-8601 duration like 'P30D').
  --color <color>                            Optional color of the article (e.g. a hex code like '#FF0000').
  --mass <mass>                              Optional mass as value+unit, e.g. '2.5kg'.
  --length-x <length-x>                      Optional dimension along X as value+unit, e.g. '10cm'.
  --length-y <length-y>                      Optional dimension along Y as value+unit, e.g. '5cm'.
  --length-z <length-z>                      Optional dimension along Z as value+unit, e.g. '2cm'.
  --variation-from <variation-from>          GUID of the source article this variation is created from. Implies the article is a variation.
  --pack-from <pack-from>                    GUID of the source article this pack is composed of. Implies the article is a pack.
  --pack-quantity <pack-quantity>            Quantity of the source article contained in the pack. Implies the article is a pack.
  --is-container                             Create the article as a container.
  --container-max-mass <container-max-mass>  Maximum mass the container can hold, as value+unit, e.g. '25kg'. Implies the article is a container.
  --barcode-ean13 <barcode-ean13>            Optional EAN-13 barcode to assign to the article.
  --kit-component <kit-component>            Kit component as '<article-guid>' or '<article-guid>:<quantity>'. Repeat the option for multiple components. Implies the article is a kit.
  --partitions <partitions>                  Partition GUIDs to associate the article with (space-separated).
  --username <username>                      Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>                      Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help                             Show help and usage information
  --server <server>                          The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose                              Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo article update`

```text
Description:
  Update an existing article by GUID. Only the options you pass are changed.
  
  Measures are written as a number followed by a unit, e.g. '2.5kg' or
  '10cm'. Use the matching --remove-* flag to clear a value.
  
  Examples:
    fargo article update 11111111-1111-1111-1111-111111111111 --name "New name"
    fargo article update 11111111-1111-1111-1111-111111111111 --mass 3kg --length-x 12cm
    fargo article update 11111111-1111-1111-1111-111111111111 --remove-mass --remove-barcode
    fargo article update 11111111-1111-1111-1111-111111111111 --barcode-ean13 1234567890128
    fargo article update 11111111-1111-1111-1111-111111111111 --add-partitions 00000000-0000-0000-0000-000000000001

Usage:
  fargo article update <guid> [options]

Arguments:
  <guid>  The article GUID.

Options:
  --name <name>                            New name for the article.
  --description <description>              New description. Leaves the existing value unchanged when omitted.
  --shelf-life <shelf-life>                New shelf life (e.g. 'P30D'). Leaves the existing value unchanged when omitted.
  --remove-shelf-life                      Clear the existing shelf life. Takes precedence over --shelf-life.
  --mass <mass>                            New mass as value+unit, e.g. '2.5kg'. Leaves the existing value unchanged when omitted.
  --remove-mass                            Clear the existing mass. Takes precedence over --mass.
  --length-x <length-x>                    New dimension along X as value+unit, e.g. '10cm'.
  --length-y <length-y>                    New dimension along Y as value+unit, e.g. '5cm'.
  --length-z <length-z>                    New dimension along Z as value+unit, e.g. '2cm'.
  --remove-length-x                        Clear the existing X dimension. Takes precedence over --length-x.
  --remove-length-y                        Clear the existing Y dimension. Takes precedence over --length-y.
  --remove-length-z                        Clear the existing Z dimension. Takes precedence over --length-z.
  --barcode-ean13 <barcode-ean13>          New EAN-13 barcode. Leaves the existing barcode unchanged when omitted.
  --remove-barcode                         Clear the existing EAN-13 barcode. Takes precedence over --barcode-ean13.
  --add-partitions <add-partitions>        Partition GUIDs to add (space-separated).
  --remove-partitions <remove-partitions>  Partition GUIDs to remove (space-separated).
  --username <username>                    Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>                    Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help                           Show help and usage information
  --server <server>                        The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose                            Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo article update-by-barcode`

```text
Description:
  Update an existing article addressed by one of its barcodes. Only the
  options you pass are changed.
  
  The barcode is given as {value}:{type} (e.g. 1234567890128:ean13).
  Measures are written as a number followed by a unit, e.g. '2.5kg'.
  
  Examples:
    fargo article update-by-barcode 1234567890128:ean13 --name "New name"
    fargo article update-by-barcode 1234567890128:ean13 --mass 3kg --remove-shelf-life

Usage:
  fargo article update-by-barcode <barcode> [options]

Arguments:
  <barcode>  The barcode in {value}:{type} format (e.g. 1234567890128:ean13).

Options:
  --name <name>                            New name for the article.
  --description <description>              New description. Leaves the existing value unchanged when omitted.
  --shelf-life <shelf-life>                New shelf life (e.g. 'P30D'). Leaves the existing value unchanged when omitted.
  --remove-shelf-life                      Clear the existing shelf life. Takes precedence over --shelf-life.
  --mass <mass>                            New mass as value+unit, e.g. '2.5kg'. Leaves the existing value unchanged when omitted.
  --remove-mass                            Clear the existing mass. Takes precedence over --mass.
  --length-x <length-x>                    New dimension along X as value+unit, e.g. '10cm'.
  --length-y <length-y>                    New dimension along Y as value+unit, e.g. '5cm'.
  --length-z <length-z>                    New dimension along Z as value+unit, e.g. '2cm'.
  --remove-length-x                        Clear the existing X dimension. Takes precedence over --length-x.
  --remove-length-y                        Clear the existing Y dimension. Takes precedence over --length-y.
  --remove-length-z                        Clear the existing Z dimension. Takes precedence over --length-z.
  --barcode-ean13 <barcode-ean13>          New EAN-13 barcode. Leaves the existing barcode unchanged when omitted.
  --remove-barcode                         Clear the existing EAN-13 barcode. Takes precedence over --barcode-ean13.
  --add-partitions <add-partitions>        Partition GUIDs to add (space-separated).
  --remove-partitions <remove-partitions>  Partition GUIDs to remove (space-separated).
  --username <username>                    Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>                    Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help                           Show help and usage information
  --server <server>                        The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose                            Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo article delete`

```text
Description:
  Delete an article by GUID.
  
  Examples:
    fargo article delete 11111111-1111-1111-1111-111111111111

Usage:
  fargo article delete <guid> [options]

Arguments:
  <guid>  The article GUID.

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo article delete-by-barcode`

```text
Description:
  Delete an article addressed by one of its barcodes.
  
  The barcode is given as {value}:{type} (e.g. 1234567890128:ean13).
  
  Examples:
    fargo article delete-by-barcode 1234567890128:ean13

Usage:
  fargo article delete-by-barcode <barcode> [options]

Arguments:
  <barcode>  The barcode in {value}:{type} format (e.g. 1234567890128:ean13).

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

