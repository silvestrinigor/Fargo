using Fargo.Core.Entities.Actors;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Authorization;

/// <summary>
/// What every authenticated request checks, read uncached so deactivation and token revocation take
/// effect immediately.
/// </summary>
/// <param name="AuthorizationStamp">
/// The current <see cref="Persistence.AuthorizationStamp"/>; <see cref="Guid.Empty"/> when the stamp
/// row is missing, in which case nothing may be cached.
/// </param>
public sealed record ActorAuthorizationState(
    Guid ActorGuid,
    bool IsActive,
    Guid AuthVersion,
    Guid AuthorizationStamp);

/// <summary>
/// Reads <see cref="ActorAuthorizationState"/> in one round trip. Scoped: token validation and the
/// actor resolver run in the same request scope, so the second read is served from memory.
/// </summary>
public sealed class ActorAuthorizationStateReader(FargoDbContext context)
{
    private readonly Dictionary<(Guid, ActorType), ActorAuthorizationState?> states = [];

    /// <returns><see langword="null"/> when no actor of that type exists.</returns>
    public async Task<ActorAuthorizationState?> GetAsync(
        Guid actorGuid,
        ActorType actorType,
        CancellationToken cancellationToken = default)
    {
        if (states.TryGetValue((actorGuid, actorType), out var known))
        {
            return known;
        }

        var query = actorType switch
        {
            ActorType.User => UserStateQuery(context, actorGuid),
            ActorType.Service => ServiceAccountStateQuery(context, actorGuid),
            _ => null,
        };

        var state = query is null ? null : await query.SingleOrDefaultAsync(cancellationToken);

        states[(actorGuid, actorType)] = state;

        return state;
    }

    // The stamp is read as a scalar subquery so the check stays a single round trip.
    internal static IQueryable<ActorAuthorizationState> UserStateQuery(FargoDbContext context, Guid userGuid)
        => context.Users
            .AsNoTracking()
            .Where(user => user.ActorGuid == userGuid)
            .Select(user => new ActorAuthorizationState(
                user.ActorGuid,
                user.Actor.IsActive,
                user.Authentication.AuthVersion,
                context.AuthorizationStamps.Select(stamp => stamp.Value).FirstOrDefault()));

    internal static IQueryable<ActorAuthorizationState> ServiceAccountStateQuery(FargoDbContext context, Guid serviceGuid)
        => context.ServiceAccounts
            .AsNoTracking()
            .Where(account => account.ActorGuid == serviceGuid)
            .Select(account => new ActorAuthorizationState(
                account.ActorGuid,
                account.Actor.IsActive,
                account.AuthVersion,
                context.AuthorizationStamps.Select(stamp => stamp.Value).FirstOrDefault()));
}
