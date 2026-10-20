# MCP Server Getting Started

This guide shows how to configure the Fargo MCP server and register it with an
MCP host.

> [!NOTE]
> The MCP server talks to a running Fargo API. Start the API first (see
> [Getting Started](../getting-started.md)) and note its base URL.

## Configuration

All configuration is supplied through environment variables. The host sets these
when it launches the server process.

| Variable | Required | Description |
| --- | --- | --- |
| `FARGO_SERVER` | Yes | Base URL of the Fargo API, e.g. `https://localhost:7563`. |
| `FARGO_USERNAME` | Conditional | User identifier (nameid) for password authentication. |
| `FARGO_PASSWORD` | Conditional | Password for password authentication. |
| `FARGO_ACCESS_TOKEN` | Conditional | A pre-issued bearer token. When set, it is used directly and `FARGO_USERNAME`/`FARGO_PASSWORD` are ignored. |

### Authentication modes

The server picks a mode based on which variables are present:

- **Password login** — set `FARGO_USERNAME` and `FARGO_PASSWORD`. The server
  logs in on the first tool call, then automatically refreshes the access token
  as it nears expiry.
- **Static token** — set `FARGO_ACCESS_TOKEN`. The server uses the token as-is
  and does not attempt to refresh it. Use this when the host already holds a
  valid token.
- **Anonymous** — set neither. Requests are sent without a bearer token; the API
  will reject any call that requires authentication with a `401`.

If `FARGO_SERVER` is missing or not a valid absolute URL, the server exits
immediately with a descriptive error.

## Running the server directly

You normally let the MCP host start the server, but you can run it manually to
verify configuration:

```bash
# PowerShell
$env:FARGO_SERVER = "https://localhost:7563"
$env:FARGO_USERNAME = "admin"
$env:FARGO_PASSWORD = "<password>"
dotnet run --project src/client/Fargo.Mcp
```

```bash
# bash
export FARGO_SERVER="https://localhost:7563"
export FARGO_USERNAME="admin"
export FARGO_PASSWORD="<password>"
dotnet run --project src/client/Fargo.Mcp
```

The process waits for MCP messages on stdin. It is designed to be driven by a
host rather than used interactively.

## Registering with an MCP host

Most hosts use a JSON configuration file that maps a server name to a launch
command and its environment. The exact file location depends on the host
(e.g. Claude Desktop's `claude_desktop_config.json`).

Using `dotnet run` against the project:

```json
{
  "mcpServers": {
    "fargo": {
      "command": "dotnet",
      "args": ["run", "--project", "/absolute/path/to/src/client/Fargo.Mcp"],
      "env": {
        "FARGO_SERVER": "https://localhost:7563",
        "FARGO_USERNAME": "admin",
        "FARGO_PASSWORD": "<password>"
      }
    }
  }
}
```

Or, after publishing the server to a self-contained executable, point `command`
at the built binary:

```bash
dotnet publish src/client/Fargo.Mcp -c Release -o ./publish/fargo-mcp
```

```json
{
  "mcpServers": {
    "fargo": {
      "command": "/absolute/path/to/publish/fargo-mcp/Fargo.Mcp",
      "env": {
        "FARGO_SERVER": "https://localhost:7563",
        "FARGO_ACCESS_TOKEN": "<access-token>"
      }
    }
  }
}
```

After the host restarts, the Fargo tools appear in its tool list and can be
invoked by the model.

## Verifying the tools

Once registered, ask the host to list available tools (or use its tool inspector)
and confirm the `fargo_*` tools appear. A simple first call is
`fargo_partitions_list`, which returns the partitions the authenticated user can
see.
