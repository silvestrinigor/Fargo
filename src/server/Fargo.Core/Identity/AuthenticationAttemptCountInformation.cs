namespace Fargo.Core.Identity;

/// <summary>
/// Represents the number of failed authentication attempts grouped by the
/// criteria used to identify them.
/// </summary>
/// <param name="IpAddressAttemptCount">
/// The number of failed attempts originating from the IP address.
/// </param>
/// <param name="ActorIdentifierAttemptCount">
/// The number of failed attempts for the actor identifier.
/// </param>
/// <param name="IpAddressAndUserIdentifierAttemptCount">
/// The number of failed attempts for the combination of the IP address and
/// the actor identifier.
/// </param>
public sealed record AuthenticationAttemptCountInformation(
    int IpAddressAttemptCount,
    int ActorIdentifierAttemptCount,
    int IpAddressAndUserIdentifierAttemptCount
);
