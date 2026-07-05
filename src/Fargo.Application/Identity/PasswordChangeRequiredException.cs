namespace Fargo.Application.Identity;

/// <summary>
/// Exception thrown when a user must change their password before accessing the system.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PasswordChangeRequiredException"/> class.
/// </remarks>
/// <param name="userGuid">
/// The identifier of the user who must change their password.
/// </param>
public sealed class PasswordChangeRequiredException(Guid userGuid)
    : Exception($"User '{userGuid}' must change their password before continuing.")
{
    /// <summary>
    /// Gets the identifier of the user who must change their password.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;
}
