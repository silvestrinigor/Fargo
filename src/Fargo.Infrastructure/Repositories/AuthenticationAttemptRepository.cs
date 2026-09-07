using Fargo.Core.Identity;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public class AuthenticationAttemptRepository(FargoDbContext context) : IAuthenticationAttemptRepository
{
    public void Add(AuthenticationAttempt authenticationAttempt)
    {
        context.AuthenticationAttempts.Add(authenticationAttempt);
    }

    public async Task<AuthenticationAttemptCountInformation> GetAuthenticationAttemptFailCountAsync(string ipAddress, string actorIdentifier, DateTimeOffset? periodStart = null, CancellationToken cancellationToken = default)
    {
        var ipAndActorFailCount = await context.AuthenticationAttempts
        .Where(x => x.IpAddress == ipAddress && x.ActorIdentifier == actorIdentifier && x.Successful == false)
        .Where(x => periodStart == null || x.OccuredAt >= periodStart)
        .CountAsync(cancellationToken);

        var actorFailCount = await context.AuthenticationAttempts
        .Where(x => x.ActorIdentifier == actorIdentifier && x.Successful == false)
        .Where(x => periodStart == null || x.OccuredAt >= periodStart)
        .CountAsync(cancellationToken);

        var ipFailCount = await context.AuthenticationAttempts
        .Where(x => x.IpAddress == ipAddress && x.Successful == false)
        .Where(x => periodStart == null || x.OccuredAt >= periodStart)
        .CountAsync(cancellationToken);

        return new AuthenticationAttemptCountInformation(
            IpAddressAndUserIdentifierAttemptCount: ipAndActorFailCount,
            IpAddressAttemptCount: ipFailCount,
            ActorIdentifierAttemptCount: actorFailCount
        );
    }

    public Task<AuthenticationAttempt?> GetLastFailAuthenticationAttemptAsync(string ipAddress, string userIdentifier, CancellationToken cancellationToken = default)
    {
        return context.AuthenticationAttempts
        .Where(x => x.IpAddress == ipAddress && x.ActorIdentifier == userIdentifier && x.Successful == false)
        .OrderByDescending(x => x.OccuredAt)
        .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<AuthenticationAttempt?> GetLastSuccessAuthenticationAttemptAsync(string ipAddress, string userIdentifier, CancellationToken cancellationToken = default)
    {
        return context.AuthenticationAttempts
        .Where(x => x.IpAddress == ipAddress && x.ActorIdentifier == userIdentifier && x.Successful == true)
        .OrderByDescending(x => x.OccuredAt)
        .FirstOrDefaultAsync(cancellationToken);
    }
}
