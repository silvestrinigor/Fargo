using System.ComponentModel.DataAnnotations;

namespace Fargo.Infrastructure.Caching;

/// <summary>
/// Configuration of the actor authorization cache, bound from the <c>ActorAuthorizationCache</c> section.
/// </summary>
public sealed class ActorAuthorizationCacheOptions
{
    public const string SectionName = "ActorAuthorizationCache";

    public ActorAuthorizationCacheProvider Provider { get; set; } = ActorAuthorizationCacheProvider.Hybrid;

    /// <summary>
    /// A safety limit only: entries are invalidated by the authorization stamp, not by expiry, so this
    /// mainly bounds how long unused entries stay in memory.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(10);
}

public enum ActorAuthorizationCacheProvider
{
    /// <summary>
    /// .NET <c>HybridCache</c>: in memory, plus any registered <c>IDistributedCache</c> (for example
    /// Redis through <c>AddStackExchangeRedisCache</c>) as a shared second level, with no code change.
    /// </summary>
    Hybrid,

    /// <summary>
    /// No caching: every resolution queries the database. Useful for debugging and measuring.
    /// </summary>
    None,
}
