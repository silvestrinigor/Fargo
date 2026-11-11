using Fargo.Mcp.Configuration;
using Fargo.Mcp.Hosting;

// Fail fast with a clear message if the server URL is missing.
var options = FargoMcpOptions.FromEnvironment();

// Default is stdio (one process = one identity, configured by environment
// variables). Pass "--transport http" (or set FARGO_MCP_TRANSPORT=http) to host
// the server over Streamable HTTP, where every request carries its own bearer
// token and gets its own scoped Fargo client.
if (McpTransport.IsHttp(args))
{
    await HttpMcpHost.RunAsync(args, options);
}
else
{
    await StdioMcpHost.RunAsync(args, options);
}
