using Fargo.ClientHttp.Extensions;
using Fargo.Mcp.Authentication;
using Fargo.Mcp.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// MCP servers communicate with the host over stdio, so stdout is reserved for
// the protocol. Route all logging to stderr to avoid corrupting the stream.
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Configuration (server URL + credentials) comes entirely from environment
// variables. Fail fast with a clear message if the server URL is missing.
var options = FargoMcpOptions.FromEnvironment();
builder.Services.AddSingleton(options);

// Register the generated Kiota client and its auth plumbing against the
// configured server. This also registers the in-memory ITokenStore the
// authenticator writes the bearer token to.
builder.Services.AddFargoHttpClient(options.Server);

// Handles login / token refresh; tools call it before each API request.
builder.Services.AddSingleton<FargoAuthenticator>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
