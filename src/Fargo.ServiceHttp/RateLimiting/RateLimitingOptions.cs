using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Fargo.ServiceHttp.RateLimiting;

/// <summary>
/// Every policy is optional in configuration: a value not supplied falls back to the defaults defined here.
/// Override under <c>RateLimiting:&lt;Policy&gt;:&lt;Field&gt;</c>.
/// </summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Applied to every request, partitioned by client IP: a coarse backstop in front of the per-endpoint
    /// policies.
    /// </summary>
    [ValidateObjectMembers]
    public RateLimitPolicyOptions Global { get; init; } = new()
    {
        PermitLimit = 300,
        WindowSeconds = 60,
        QueueLimit = 0,
    };

    /// <summary>
    /// General authenticated resource endpoints (articles, items, partitions, users, user groups, audit logs),
    /// partitioned by the authenticated user.
    /// </summary>
    [ValidateObjectMembers]
    public RateLimitPolicyOptions GeneralAuthenticated { get; init; } = new()
    {
        PermitLimit = 100,
        WindowSeconds = 60,
        QueueLimit = 0,
    };

    /// <summary>
    /// The login endpoint, partitioned by client IP. Kept strict to slow credential-stuffing and brute force.
    /// </summary>
    [ValidateObjectMembers]
    public RateLimitPolicyOptions IdentityLogin { get; init; } = new()
    {
        PermitLimit = 10,
        WindowSeconds = 60,
        QueueLimit = 0,
    };

    [ValidateObjectMembers]
    public RateLimitPolicyOptions IdentityRefresh { get; init; } = new()
    {
        PermitLimit = 30,
        WindowSeconds = 60,
        QueueLimit = 0,
    };

    /// <summary>
    /// The remaining identity endpoints (logout, change password), partitioned by client IP.
    /// </summary>
    [ValidateObjectMembers]
    public RateLimitPolicyOptions IdentityDefault { get; init; } = new()
    {
        PermitLimit = 10,
        WindowSeconds = 60,
        QueueLimit = 0,
    };
}

public sealed class RateLimitPolicyOptions
{
    [Range(1, int.MaxValue)]
    public int PermitLimit { get; init; }

    [Range(1, int.MaxValue)]
    public int WindowSeconds { get; init; }

    /// <summary>
    /// Requests queued once the limit is reached; 0 rejects immediately, which suits an API.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int QueueLimit { get; init; }

    public TimeSpan Window => TimeSpan.FromSeconds(WindowSeconds);
}
