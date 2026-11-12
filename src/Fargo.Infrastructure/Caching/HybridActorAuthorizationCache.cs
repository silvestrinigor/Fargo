using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace Fargo.Infrastructure.Caching;

/// <summary>
/// <see cref="IActorAuthorizationCache"/> backed by .NET <see cref="HybridCache"/>, which also
/// prevents concurrent requests from rebuilding the same entry at once.
/// </summary>
public sealed class HybridActorAuthorizationCache(
    HybridCache cache,
    IOptions<ActorAuthorizationCacheOptions> options) : IActorAuthorizationCache
{
    private readonly HybridCacheEntryOptions entryOptions = new()
    {
        Expiration = options.Value.Expiration,
        LocalCacheExpiration = options.Value.Expiration,
    };

    public async Task<CachedActorAuthorization> GetOrCreateAsync(
        ActorAuthorizationCacheKey key,
        Func<CancellationToken, Task<CachedActorAuthorization>> factory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(factory);

        return await cache.GetOrCreateAsync(
            ToCacheKey(key),
            factory,
            static async (create, ct) => await create(ct),
            entryOptions,
            cancellationToken: cancellationToken);
    }

    internal static string ToCacheKey(ActorAuthorizationCacheKey key)
        => $"fargo:actor-authorization:{(int)key.ActorType}:{key.ActorGuid:N}:{key.AuthorizationStamp:N}";
}
