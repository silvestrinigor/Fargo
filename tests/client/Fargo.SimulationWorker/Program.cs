using Fargo.ClientHttp.Extensions;
using Fargo.SimulationWorker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Long-running client-activity simulation worker for the Fargo system. It drives
// the real HTTP API of a running instance in an infinite loop, simulating a busy
// warehouse: several concurrent virtual operators, each with its own role
// (receiver, put-away, picker, bulk handler, auditor), continuously creating
// articles and items, moving them between containers, recording event-tracked
// bulk movements, and reading back inventory / movements / audit logs. This
// generates a realistic, never-ending stream of client/user history.
//
// Configuration lives in appsettings.json (the "SimulationWorker" section).
// Host.CreateApplicationBuilder layers, in increasing precedence:
//   appsettings.json -> appsettings.{Environment}.json -> environment
//   variables -> command-line arguments
// so any value can be overridden without touching the binary, e.g.:
//   dotnet run --project tests/client/Fargo.SimulationWorker -- --SimulationWorker:Operators=8
//   dotnet run --project tests/client/Fargo.SimulationWorker -- --SimulationWorker:MaxDurationSeconds=30
//   $env:SimulationWorker__Server = "https://localhost:7563"

var builder = Host.CreateApplicationBuilder(args);

// Load configuration from appsettings.json located next to the binary
// (CopyToOutputDirectory), regardless of the current working directory. Env vars
// and command-line arguments still layer on top with higher precedence.
builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// Bind and validate configuration up front so the process fails fast with a
// clear message when a required setting is missing or nonsensical.
builder.Services
    .AddOptions<SimulationOptions>()
    .Bind(builder.Configuration.GetSection(SimulationOptions.SectionName))
    .Validate(o => o.Server is not null, "SimulationWorker:Server must be provided.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Username), "SimulationWorker:Username must be provided.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Password), "SimulationWorker:Password must be provided.")
    .Validate(o => o.Operators >= 1, "SimulationWorker:Operators must be at least 1.")
    .Validate(o => o.MinThinkTimeMs >= 0, "SimulationWorker:MinThinkTimeMs cannot be negative.")
    .Validate(o => o.MaxThinkTimeMs >= o.MinThinkTimeMs,
        "SimulationWorker:MaxThinkTimeMs must be >= MinThinkTimeMs.")
    .Validate(o => o.MaxDurationSeconds >= 0, "SimulationWorker:MaxDurationSeconds cannot be negative.")
    .Validate(o => o.MaxIterationsPerOperator >= 0,
        "SimulationWorker:MaxIterationsPerOperator cannot be negative.")
    .ValidateOnStart();

// The Fargo HTTP client needs the base address at registration time, so read it
// from the bound configuration before building the container. This registered
// client + its singleton ITokenStore belong to the bootstrap admin session.
var options = builder.Configuration
    .GetSection(SimulationOptions.SectionName)
    .Get<SimulationOptions>()
    ?? throw new InvalidOperationException(
        $"Missing '{SimulationOptions.SectionName}' configuration section.");

builder.Services.AddFargoHttpClient(options.Server);

builder.Services.AddHostedService<SimulationService>();

using var host = builder.Build();

await host.RunAsync();
