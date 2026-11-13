using Fargo.Client.Extensions;
using Fargo.Mcp.Authentication;
using Fargo.Mcp.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fargo.Mcp.Hosting;

/// <summary>
/// Hosts the MCP server over stdio. One process serves one identity, so the
/// Fargo client, token store and authenticator are singletons.
/// </summary>
internal static class StdioMcpHost
{
    public static async Task RunAsync(string[] args, FargoMcpOptions options)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // MCP servers communicate with the host over stdio, so stdout is reserved for
        // the protocol. Route all logging to stderr to avoid corrupting the stream.
        builder.Logging.AddConsole(consoleLogOptions =>
        {
            consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        builder.Services.AddSingleton(options);

        builder.Services.AddFargoHttpClient(options.Server);

        builder.Services.AddSingleton<FargoAuthenticator>();

        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        await builder.Build().RunAsync();
    }
}
