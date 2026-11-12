using Fargo.ClientHttp.ErrorHandling;

namespace Fargo.SimulationWorker;

/// <summary>
/// Drives one simulated operator forever (or until the configured bounds or
/// cancellation). It authenticates as the operator, then repeatedly: asks the
/// persona router for the next action, runs it through the rate-limit policy,
/// records metrics, and waits a randomised think-time. Non-fatal API errors are
/// logged and counted so a single bad call never stops the operator.
/// </summary>
internal sealed class OperatorLoop(
    SimulationOptions options,
    FargoClientBuilder clientBuilder,
    OperatorActions actions,
    PersonaRouter router,
    Metrics metrics)
{
    private readonly SimulationOptions _options = options;
    private readonly FargoClientBuilder _clientBuilder = clientBuilder;
    private readonly OperatorActions _actions = actions;
    private readonly PersonaRouter _router = router;
    private readonly Metrics _metrics = metrics;

    public async Task RunAsync(OperatorAccount account, CancellationToken cancellationToken)
    {
        var random = new Random(_options.Seed ^ (account.Index * 2654435761u).GetHashCode());
        var rateLimit = new RateLimitPolicy(_options, _metrics, random);

        OperatorContext ctx;
        try
        {
            var client = await _clientBuilder.CreateAuthenticatedClientAsync(
                account.Nameid, _options.NewUserPassword, cancellationToken);
            ctx = new OperatorContext(account, client, random);
            Log.Success(ctx.Who, $"authenticated as '{account.Nameid}'");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Error($"{account.Role}#{account.Index:D2}", $"could not authenticate: {ex.Message}");
            return;
        }

        var iterations = 0L;
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var action = await rateLimit.ExecuteAsync<string>(
                    async () => await _router.RunNextAsync(ctx, cancellationToken), cancellationToken);

                if (action is not null)
                {
                    _metrics.RecordAction(action);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (FargoApiException ex)
            {
                _metrics.RecordError();
                Log.Warn(ctx.Who, $"action failed ({ex.StatusCode}): {ex.Message}");
            }
            catch (Exception ex)
            {
                _metrics.RecordError();
                Log.Warn(ctx.Who, $"unexpected error: {ex.Message}");
            }

            iterations++;
            if (_options.MaxIterationsPerOperator > 0 && iterations >= _options.MaxIterationsPerOperator)
            {
                Log.Info(ctx.Who, $"reached iteration cap ({_options.MaxIterationsPerOperator}); stopping.");
                break;
            }

            await ThinkAsync(ctx, cancellationToken);
        }
    }

    private async Task ThinkAsync(OperatorContext ctx, CancellationToken cancellationToken)
    {
        var min = Math.Max(0, _options.MinThinkTimeMs);
        var max = Math.Max(min + 1, _options.MaxThinkTimeMs);
        var delay = ctx.Random.Next(min, max);
        try
        {
            await Task.Delay(delay, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Shutting down; swallow so the loop exits cleanly.
        }
    }
}
