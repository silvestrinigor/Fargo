using Fargo.ClientHttp;
using Fargo.ClientHttp.Authentication;
using Fargo.ClientHttp.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Fargo.SimulationWorker;

/// <summary>
/// The long-running simulation worker. As a <see cref="BackgroundService"/> it:
///  1. authenticates as the bootstrap admin,
///  2. builds the world (partitions, role groups, operator users),
///  3. launches one concurrent loop per operator,
///  4. prints a periodic metrics summary, and
///  5. runs until Ctrl+C, the optional duration cap, or all operators stop,
///     then prints a final summary.
/// </summary>
internal sealed class SimulationService(
    IHostApplicationLifetime lifetime,
    FargoApiClient adminClient,
    ITokenStore adminTokenStore,
    IOptions<SimulationOptions> options) : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime = lifetime;
    private readonly FargoApiClient _admin = adminClient;
    private readonly ITokenStore _adminTokenStore = adminTokenStore;
    private readonly SimulationOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var names = new Names(_options.RunTag);
        var check = new Check();
        var metrics = new Metrics();
        var clientBuilder = new FargoClientBuilder(_options.Server);

        PrintBanner(names.RunTag);

        try
        {
            await LoginAdminAsync(stoppingToken);

            var bootstrapper = new WorldBootstrapper(_admin, clientBuilder, _options, names, check);
            var world = await bootstrapper.BootstrapAsync(stoppingToken);

            await RunOperatorsAsync(world, clientBuilder, names, check, metrics, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            Log.Warn("sim", "cancellation requested; shutting down.");
        }
        catch (Exception ex)
        {
            Log.Error("sim", $"fatal error: {ex.Message}");
            Log.Info("sim", ex.ToString());
        }
        finally
        {
            Log.Banner("SIMULATION FINAL SUMMARY");
            Log.Metrics(metrics.Summarize());
            Log.Metrics($"             consistency checks: {check.Passed} passed, {check.Failed} failed");
            _lifetime.StopApplication();
        }
    }

    private async Task RunOperatorsAsync(
        SimulationWorld world,
        FargoClientBuilder clientBuilder,
        Names names,
        Check check,
        Metrics metrics,
        CancellationToken stoppingToken)
    {
        // A linked token lets the optional duration cap stop the run alongside
        // the host's shutdown signal.
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        if (_options.MaxDurationSeconds > 0)
        {
            linked.CancelAfter(TimeSpan.FromSeconds(_options.MaxDurationSeconds));
            Log.Info("sim", $"duration cap: {_options.MaxDurationSeconds}s.");
        }

        var actions = new OperatorActions(_options, names, check);
        var router = new PersonaRouter(actions);

        Log.Banner($"SIMULATION RUNNING ({world.Operators.Count} operators)");

        var operatorTasks = world.Operators.Select(account =>
        {
            var loop = new OperatorLoop(_options, clientBuilder, actions, router, metrics);
            return loop.RunAsync(account, linked.Token);
        }).ToArray();

        var metricsTask = ReportMetricsAsync(metrics, check, linked.Token);

        // The run ends when every operator loop ends (iteration cap) or the
        // linked token fires (Ctrl+C / duration cap).
        await Task.WhenAll(operatorTasks);
        await linked.CancelAsync();

        try
        {
            await metricsTask;
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown.
        }
    }

    private async Task ReportMetricsAsync(Metrics metrics, Check check, CancellationToken cancellationToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, _options.MetricsIntervalSeconds));
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(interval, cancellationToken);
            Log.Metrics(metrics.Summarize());
            if (_options.EnableConsistencyChecks && (check.Passed + check.Failed) > 0)
            {
                Log.Metrics($"             consistency: {check.Passed} ok, {check.Failed} failed");
            }
        }
    }

    private async Task LoginAdminAsync(CancellationToken cancellationToken)
    {
        Log.Info("setup", $"Authenticating bootstrap admin '{_options.Username}'.");

        var auth = await _admin.V1.Identity.Login.PostAsync(
            new IdentityLoginDto
            {
                Nameid = _options.Username,
                Password = _options.Password,
            },
            cancellationToken: cancellationToken);

        if (auth?.AccessToken is null || auth.RefreshToken is null || auth.ExpiresAt is null)
        {
            throw new InvalidOperationException("Admin login did not return a complete token set.");
        }

        await _adminTokenStore.SetAsync(
            new AuthTokens(auth.AccessToken, auth.RefreshToken, auth.ExpiresAt.Value),
            cancellationToken);
    }

    private void PrintBanner(string runTag)
    {
        Log.Banner("Fargo client simulation worker");
        Log.Info("sim", $"Server            : {_options.Server}");
        Log.Info("sim", $"Admin             : {_options.Username}");
        Log.Info("sim", $"Operators         : {_options.Operators}");
        Log.Info("sim", $"Think-time        : {_options.MinThinkTimeMs}-{_options.MaxThinkTimeMs} ms");
        Log.Info("sim", $"Entity flow       : {_options.EnableEntityFlow}");
        Log.Info("sim", $"Event flow        : {_options.EnableEventFlow}");
        Log.Info("sim", $"Auditor flow      : {_options.EnableAuditorFlow}");
        Log.Info("sim", $"Consistency checks: {_options.EnableConsistencyChecks}");
        var duration = _options.MaxDurationSeconds > 0 ? $"{_options.MaxDurationSeconds}s" : "infinite";
        var iterations = _options.MaxIterationsPerOperator > 0
            ? _options.MaxIterationsPerOperator.ToString()
            : "infinite";
        Log.Info("sim", $"Duration / iters  : {duration} / {iterations}");
        Log.Info("sim", $"Run tag           : {runTag}");
    }
}
