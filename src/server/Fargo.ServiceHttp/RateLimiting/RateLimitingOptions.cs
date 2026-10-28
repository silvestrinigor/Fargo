using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Fargo.ServiceHttp.RateLimiting;

/// <summary>
/// Configuration for the HTTP service's rate limiting, bound from the
/// <c>RateLimiting</c> configuration section.
/// </summary>
/// <remarks>
/// Every policy is optional in configuration: any value not supplied falls back
/// to the defaults defined here, so the service has a safe, working rate-limit
/// configuration out of the box. Override any field under
/// <c>RateLimiting:&lt;Policy&gt;:&lt;Field&gt;</c> in <c>appsettings.json</c> or
/// environment variables.
/// </remarks>
public sealed class RateLimitingOptions
{
    /// <summary>
    /// The configuration section name these options bind from.
    /// </summary>
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Whether rate limiting is enabled. Defaults to <see langword="true"/>.
    /// Set to <see langword="false"/> to disable all limiting (for example in
    /// local development or load testing).
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// The global limiter applied to every request, partitioned by client IP.
    /// Acts as a coarse backstop in front of the per-endpoint policies.
    /// </summary>
    [ValidateObjectMembers]
    public RateLimitPolicyOptions Global { get; init; } = new()
    {
        PermitLimit = 300,
        WindowSeconds = 60,
        QueueLimit = 0,
    };

    /// <summary>
    /// The limiter for general authenticated resource endpoints (articles,
    /// items, partitions, users, user groups, audit logs), partitioned by the
    /// authenticated user.
    /// </summary>
    [ValidateObjectMembers]
    public RateLimitPolicyOptions GeneralAuthenticated { get; init; } = new()
    {
        PermitLimit = 100,
        WindowSeconds = 60,
        QueueLimit = 0,
    };

    /// <summary>
    /// The limiter for the login endpoint, partitioned by client IP. Kept strict
    /// to slow credential-stuffing and brute-force attempts.
    /// </summary>
    [ValidateObjectMembers]
    public RateLimitPolicyOptions IdentityLogin { get; init; } = new()
    {
        PermitLimit = 10,
        WindowSeconds = 60,
        QueueLimit = 0,
    };

    /// <summary>
    /// The limiter for the token refresh endpoint, partitioned by client IP.
    /// </summary>
    [ValidateObjectMembers]
    public RateLimitPolicyOptions IdentityRefresh { get; init; } = new()
    {
        PermitLimit = 30,
        WindowSeconds = 60,
        QueueLimit = 0,
    };

    /// <summary>
    /// The limiter for the remaining identity endpoints (logout, change
    /// password), partitioned by client IP.
    /// </summary>
    [ValidateObjectMembers]
    public RateLimitPolicyOptions IdentityDefault { get; init; } = new()
    {
        PermitLimit = 10,
        WindowSeconds = 60,
        QueueLimit = 0,
    };
}

/// <summary>
/// A single fixed-window rate-limit policy.
/// </summary>
public sealed class RateLimitPolicyOptions
{
    /// <summary>
    /// The maximum number of permitted requests within <see cref="WindowSeconds"/>.
    /// Must be at least 1.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PermitLimit { get; init; }

    /// <summary>
    /// The length of the fixed window, in seconds. Must be at least 1.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int WindowSeconds { get; init; }

    /// <summary>
    /// The maximum number of requests queued when the limit is reached. Defaults
    /// to 0 (reject immediately), which is the appropriate behavior for an API.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int QueueLimit { get; init; }

    /// <summary>
    /// Gets the window as a <see cref="TimeSpan"/>.
    /// </summary>
    public TimeSpan Window => TimeSpan.FromSeconds(WindowSeconds);
}
