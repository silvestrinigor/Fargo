namespace Fargo.Domain.Users;

/// <summary>
/// Exception thrown when an operation is attempted with an inactive user.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserInactiveFargoDomainException"/> class.
/// </remarks>
/// <param name="userGuid">
/// The identifier of the inactive user.
/// </param>
public sealed class UserInactiveFargoDomainException(Guid userGuid)
    : FargoDomainException($"The user '{userGuid}' is inactive and cannot perform this operation.")
{
    /// <summary>
    /// Gets the GUID of the user that is inactive.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;
}
