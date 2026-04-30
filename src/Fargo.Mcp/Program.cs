using Fargo.Sdk.Authentication;
using Fargo.Sdk.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var server = Environment.GetEnvironmentVariable("FARGO_SERVER")
    ?? throw new InvalidOperationException("FARGO_SERVER environment variable is required.");
var nameid = Environment.GetEnvironmentVariable("FARGO_NAMEID")
    ?? throw new InvalidOperationException("FARGO_NAMEID environment variable is required.");
var password = Environment.GetEnvironmentVariable("FARGO_PASSWORD")
    ?? throw new InvalidOperationException("FARGO_PASSWORD environment variable is required.");
var apiKey = Environment.GetEnvironmentVariable("FARGO_API_KEY");

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Warning);

builder.Services.AddFargoSdk(o =>
    {
        o.Server = server;
        o.ApiKey = apiKey;
    }, ServiceLifetime.Singleton);
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

var auth = app.Services.GetRequiredService<IAuthenticationService>();
await auth.LogInAsync(nameid, password);

await app.RunAsync();
