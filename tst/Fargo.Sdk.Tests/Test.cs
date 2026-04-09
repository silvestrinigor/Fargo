using Fargo.Sdk;
using Microsoft.Extensions.Logging;

using ILoggerFactory log = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Trace);
});

var sdk = new Engine(log);

await sdk.LogInAsync("https://localhost:7563", "admin", "HJLBaQLIcinDp6KrqRjjgQ@");

await sdk.LogOutAsync();

await sdk.LogInAsync("https://localhost:7563", "admin", "HJLBaQLIcinDp6KrqRjjgQ@");

await sdk.LogOutAsync();
