namespace Fargo.SimulationWorker;

/// <summary>
/// Runtime configuration for the client-activity simulation worker.
/// </summary>
/// <remarks>
/// Values are provided through application configuration — the
/// <c>SimulationWorker</c> section of <c>appsettings.json</c> (and its
/// environment-specific overrides) or matching environment variables — and are
/// bound and validated at startup. They are intentionally not hard-coded.
/// </remarks>
public sealed class SimulationOptions
{
    /// <summary>The configuration section name used for binding these options.</summary>
    public const string SectionName = "SimulationWorker";

    /// <summary>Base address of the running Fargo HTTP API.</summary>
    public required Uri Server { get; init; }

    /// <summary>Username of the bootstrap administrator used to set up the world.</summary>
    public required string Username { get; init; }

    /// <summary>Password of the bootstrap administrator.</summary>
    public required string Password { get; init; }

    /// <summary>
    /// Number of concurrent virtual operators to simulate. Each operator gets its
    /// own authenticated session (and therefore its own per-user rate-limit
    /// bucket), so raising this multiplies total throughput.
    /// </summary>
    public int Operators { get; init; } = 4;

    /// <summary>
    /// Password assigned to every simulated operator user. Must satisfy the
    /// server's password policy (9+ chars, a letter, a digit and a special char).
    /// </summary>
    public string NewUserPassword { get; init; } = "Simulate123@";

    /// <summary>
    /// A short unique suffix appended to generated names (users, groups,
    /// partitions, articles) so repeated runs against the same database do not
    /// collide on unique identifiers. When empty, a timestamp-based suffix is
    /// generated.
    /// </summary>
    public string RunTag { get; init; } = string.Empty;

    /// <summary>Seed for the random generators, so runs are reproducible.</summary>
    public int Seed { get; init; } = 20260101;

    /// <summary>Minimum think-time between an operator's actions, in milliseconds.</summary>
    public int MinThinkTimeMs { get; init; } = 250;

    /// <summary>Maximum think-time between an operator's actions, in milliseconds.</summary>
    public int MaxThinkTimeMs { get; init; } = 1500;

    /// <summary>
    /// Optional cap on the total wall-clock duration of the simulation, in
    /// seconds. When <c>0</c> (the default) the simulation runs forever until
    /// cancelled (Ctrl+C). Useful for CI/smoke runs.
    /// </summary>
    public int MaxDurationSeconds { get; init; }

    /// <summary>
    /// Optional cap on the number of actions EACH operator performs. When
    /// <c>0</c> (the default) operators loop forever until cancelled. Useful for
    /// CI/smoke runs.
    /// </summary>
    public int MaxIterationsPerOperator { get; init; }

    /// <summary>Interval, in seconds, between periodic metrics summaries.</summary>
    public int MetricsIntervalSeconds { get; init; } = 15;

    /// <summary>
    /// When true, operators periodically read back a sample of their writes and
    /// assert self-consistency (logged but never fatal). When false the worker
    /// only generates history.
    /// </summary>
    public bool EnableConsistencyChecks { get; init; } = true;

    /// <summary>When true, the entity-tracked containment personas are enabled.</summary>
    public bool EnableEntityFlow { get; init; } = true;

    /// <summary>When true, the event-tracked bulk-goods persona is enabled.</summary>
    public bool EnableEventFlow { get; init; } = true;

    /// <summary>When true, the read-only auditor persona is enabled.</summary>
    public bool EnableAuditorFlow { get; init; } = true;

    /// <summary>Initial backoff applied on a 429 (rate-limited) response, in milliseconds.</summary>
    public int RateLimitBaseBackoffMs { get; init; } = 1000;

    /// <summary>Maximum backoff applied on repeated 429 responses, in milliseconds.</summary>
    public int RateLimitMaxBackoffMs { get; init; } = 30000;
}
