using Fargo.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var server = Environment.GetEnvironmentVariable("FARGO_SERVER")
    ?? throw new InvalidOperationException("FARGO_SERVER environment variable is required.");
var nameid = Environment.GetEnvironmentVariable("FARGO_NAMEID")
    ?? throw new InvalidOperationException("FARGO_NAMEID environment variable is required.");
var password = Environment.GetEnvironmentVariable("FARGO_PASSWORD")
    ?? throw new InvalidOperationException("FARGO_PASSWORD environment variable is required.");

var engine = new Engine();
await engine.LogInAsync(server, nameid, password);

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Warning);

builder.Services.AddSingleton<IEngine>(engine);
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
