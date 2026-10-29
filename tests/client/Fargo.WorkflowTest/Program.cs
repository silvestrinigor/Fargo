using Fargo.ClientHttp;
using Fargo.ClientHttp.Authentication;
using Fargo.ClientHttp.Extensions;
using Fargo.WorkflowTest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

// End-to-end workflow driver for the Fargo system. It exercises the real HTTP
// API of a running instance: create articles + items, move items between
// containers, then verify the resulting state.
//
// Configuration lives in appsettings.json (the "Workflow" section), not in
// code. Host.CreateApplicationBuilder layers, in increasing precedence:
//   appsettings.json -> appsettings.{Environment}.json -> environment
//   variables -> command-line arguments
// so any value can be overridden without touching the binary, e.g.:
//   dotnet run --project tests/client/Fargo.WorkflowTest -- --Workflow:Server=http://localhost:5534
//   $env:Workflow__Moves = 50

var builder = Host.CreateApplicationBuilder(args);

// Load configuration from appsettings.json located next to the binary
// (CopyToOutputDirectory), regardless of the current working directory that
// `dotnet run` or a shell happens to use as the content root. Environment
// variables and command-line arguments still layer on top with higher
// precedence (added by Host.CreateApplicationBuilder).
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

// Bind and validate the workflow configuration up front so the process fails
// fast with a clear message when a required setting is missing.
builder.Services
    .AddOptions<WorkflowOptions>()
    .Bind(builder.Configuration.GetSection(WorkflowOptions.SectionName))
    .Validate(o => o.Server is not null, "Workflow:Server must be provided.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Username), "Workflow:Username must be provided.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Password), "Workflow:Password must be provided.")
    .Validate(o => o.Containers >= 2, "Workflow:Containers must be at least 2.")
    .Validate(o => o.Articles >= 1, "Workflow:Articles must be at least 1.")
    .Validate(o => o.ItemsPerArticle >= 1, "Workflow:ItemsPerArticle must be at least 1.")
    .Validate(o => o.Moves >= 0, "Workflow:Moves cannot be negative.")
    .ValidateOnStart();

// The Fargo HTTP client needs the base address at registration time, so read it
// from the bound configuration before building the container.
var options = builder.Configuration
    .GetSection(WorkflowOptions.SectionName)
    .Get<WorkflowOptions>()
    ?? throw new InvalidOperationException(
        $"Missing '{WorkflowOptions.SectionName}' configuration section.");

builder.Services.AddFargoHttpClient(options.Server);

builder.Services.AddSingleton<WorkflowRunner>();

using var host = builder.Build();

var runner = host.Services.GetRequiredService<WorkflowRunner>();

// Allow Ctrl+C to cancel the in-flight workflow gracefully.
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    return await runner.RunAsync(cts.Token);
}
catch (OptionsValidationException ex)
{
    foreach (var failure in ex.Failures)
    {
        Log.Error(failure);
    }
    return 2;
}
catch (OperationCanceledException)
{
    Log.Warn("Cancelled.");
    return 130;
}
catch (Exception ex)
{
    Log.Error($"Workflow failed: {ex.Message}");
    Log.Info(ex.ToString());
    return 1;
}
