# Realtime

Command reference for `fargo realtime`, generated from the CLI's help output.

## `fargo realtime`

```text
Description:
  Watch changes pushed live by the Fargo real-time service.

Usage:
  fargo realtime [command] [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

Commands:
  watch  Print each change, as it happens, to the entities your user can read.
  
         Connects to the Fargo real-time service and keeps running until Ctrl+C.
         Only changes to entities in partitions you can access are shown; today
         these are article changes (created, edited, deleted). The line shows
         what changed; read the entity with the matching get command for its
         details.
  
         Changes are written to standard output; connection messages go to
         standard error, so --json output can be piped. Changes made while the
         connection is down are not replayed.
  
         The access token is refreshed automatically while watching. Requires
         a logged-in session ('fargo identity login') or --username/--password.
  
         Examples:
           fargo realtime watch
           fargo realtime watch --json
           fargo realtime watch --realtime-server https://localhost:7571

```

## `fargo realtime watch`

```text
Description:
  Print each change, as it happens, to the entities your user can read.
  
  Connects to the Fargo real-time service and keeps running until Ctrl+C.
  Only changes to entities in partitions you can access are shown; today
  these are article changes (created, edited, deleted). The line shows
  what changed; read the entity with the matching get command for its
  details.
  
  Changes are written to standard output; connection messages go to
  standard error, so --json output can be piped. Changes made while the
  connection is down are not replayed.
  
  The access token is refreshed automatically while watching. Requires
  a logged-in session ('fargo identity login') or --username/--password.
  
  Examples:
    fargo realtime watch
    fargo realtime watch --json
    fargo realtime watch --realtime-server https://localhost:7571

Usage:
  fargo realtime watch [options]

Options:
  --realtime-server <realtime-server>  The real-time service URL. Defaults to 'fargo config get-realtime-server'.
  --json                               Write each change as a single-line JSON object.
  --username <username>                Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>                Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help                       Show help and usage information
  --server <server>                    The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose                        Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

