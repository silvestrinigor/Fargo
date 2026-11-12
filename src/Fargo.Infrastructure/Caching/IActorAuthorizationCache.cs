using Fargo.Core.Entities.Actors;
using System.ComponentModel;

namespace Fargo.Infrastructure.Caching;

/// <summary>
/// Caches actor authorization data, independent of the cache technology. Replace the registered
/// implementation to move to another backend.
/// </summary>
/// <remarks>
/// Entries never need to be removed: a change produces a new authorization stamp, so the key changes
/// and old entries are simply never read again until they expire.
/// </remarks>
public interface IActorAuthorizationCache
{
    Task<CachedActorAuthorization> GetOrCreateAsync(
        ActorAuthorizationCacheKey key,
        Func<CancellationToken, Task<CachedActorAuthorization>> factory,
        CancellationToken cancellationToken = default);
}

public sealed record ActorAuthorizationCacheKey(ActorType ActorType, Guid ActorGuid, Guid AuthorizationStamp);

/// <summary>
/// The cached value: plain lists, so any backend can serialize it.
/// </summary>
[ImmutableObject(true)]
public sealed record CachedActorAuthorization(
    IReadOnlyList<ActionType> Permissions,
    IReadOnlyList<Guid> PartitionAccessGuids);

/// <summary>
/// Caches nothing: every call runs the factory.
/// </summary>
public sealed class NoActorAuthorizationCache : IActorAuthorizationCache
{
    public Task<CachedActorAuthorization> GetOrCreateAsync(
        ActorAuthorizationCacheKey key,
        Func<CancellationToken, Task<CachedActorAuthorization>> factory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return factory(cancellationToken);
    }
}
