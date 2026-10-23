# Identity

Command reference for `fargo identity`, generated from the CLI's help output.

## `fargo identity`

```text
Description:
  Authenticate with the Fargo server (login, logout, change password).

Usage:
  fargo identity [command] [options]

Options:
  -?, -h, --help     Show help and usage information
  --server <server>  The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose      Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

Commands:
  login            Authenticate with the Fargo server and store the session token.
  
                   If --password is omitted you will be prompted to type it securely
                   (the input is not echoed).
  
                   Examples:
                     fargo identity login --username admin
                     fargo identity login --username admin --password 'S3cr3t!'
  logout           Log out from the Fargo server and clear the stored session token.
  
                   Examples:
                     fargo identity logout
  change-password  Change the password for an account.
  
                   If --current-password or --new-password are omitted you will be
                   prompted to type them securely (the input is not echoed).
  
                   Examples:
                     fargo identity change-password --username admin
                     fargo identity change-password --username admin --current-password old --new-password new

```

## `fargo identity login`

```text
Description:
  Authenticate with the Fargo server and store the session token.
  
  If --password is omitted you will be prompted to type it securely
  (the input is not echoed).
  
  Examples:
    fargo identity login --username admin
    fargo identity login --username admin --password 'S3cr3t!'

Usage:
  fargo identity login [options]

Options:
  --username <username> (REQUIRED)  Username.
  --password <password>             Password. If omitted, you will be prompted to enter it securely.
  -?, -h, --help                    Show help and usage information
  --server <server>                 The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose                     Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo identity logout`

```text
Description:
  Log out from the Fargo server and clear the stored session token.
  
  Examples:
    fargo identity logout

Usage:
  fargo identity logout [options]

Options:
  -?, -h, --help     Show help and usage information
  --server <server>  The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose      Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

## `fargo identity change-password`

```text
Description:
  Change the password for an account.
  
  If --current-password or --new-password are omitted you will be
  prompted to type them securely (the input is not echoed).
  
  Examples:
    fargo identity change-password --username admin
    fargo identity change-password --username admin --current-password old --new-password new

Usage:
  fargo identity change-password [options]

Options:
  --username <username> (REQUIRED)       Username (nameid) of the account.
  --current-password <current-password>  Current password. If omitted, you will be prompted securely.
  --new-password <new-password>          New password. If omitted, you will be prompted securely.
  -?, -h, --help                         Show help and usage information
  --server <server>                      The Fargo server URL to connect to (e.g. https://localhost:7563). Overrides the configured server for this invocation.
  -v, --verbose                          Show detailed diagnostics for errors, including exception types, inner exceptions, and stack traces.

```

