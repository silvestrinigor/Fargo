namespace Fargo.Core.Identity;

public sealed record AuthenticationAttemptCountInformation(
    int IpAddressAttemptCount,
    int ActorIdentifierAttemptCount,
    int IpAddressAndUserIdentifierAttemptCount
);
