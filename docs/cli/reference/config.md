# Config

Command reference for `fargo config`, generated from the CLI's help output.

## `fargo config`

```text
Description:
  Manage CLI configuration (the server URL this CLI connects to).

Usage:
  fargo config [command] [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

Commands:
  set-server  Set and persist the Fargo server URL the CLI connects to by default.
  
              This is used when a command does not pass --server explicitly.
  
              Examples:
                fargo config set-server --url https://localhost:7563
  get-server  Print the configured default Fargo server URL.
  
              Examples:
                fargo config get-server
  show        Show the current CLI configuration.
  
              Examples:
                fargo config show

```

## `fargo config set-server`

```text
Description:
  Set and persist the Fargo server URL the CLI connects to by default.
  
  This is used when a command does not pass --server explicitly.
  
  Examples:
    fargo config set-server --url https://localhost:7563

Usage:
  fargo config set-server [options]

Options:
  --url <url> (REQUIRED)  The Fargo server URL (e.g. https://localhost:7563).
  --username <username>   Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>   Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help          Show help and usage information
  --server <server>       The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose           Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo config get-server`

```text
Description:
  Print the configured default Fargo server URL.
  
  Examples:
    fargo config get-server

Usage:
  fargo config get-server [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo config show`

```text
Description:
  Show the current CLI configuration.
  
  Examples:
    fargo config show

Usage:
  fargo config show [options]

Options:
  --username <username>  Username to authenticate with for this invocation. Provide together with --password to sign in just for this command.
  --password <password>  Password to authenticate with for this invocation. Provide together with --username to sign in just for this command.
  -?, -h, --help         Show help and usage information
  --server <server>      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

