namespace Fargo.Core.Identity.Authentication;

/// <summary>
/// Failed attempt counts by IP address, by actor identifier, and by both combined.
/// </summary>
public sealed record AuthenticationAttemptCountInformation(
    int IpAddressAttemptCount,
    int ActorIdentifierAttemptCount,
    int IpAddressAndUserIdentifierAttemptCount
);
