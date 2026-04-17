namespace Fargo.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user attempts to delete their own account.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserCannotDeleteSelfFargoDomainException"/> class.
/// </remarks>
/// <param name="userGuid">
/// The identifier of the user that attempted to delete themselves.
/// </param>
public sealed class UserCannotDeleteSelfFargoDomainException(Guid userGuid)
            : FargoDomainException($"User '{userGuid}' cannot delete their own account.")
{
    /// <summary>
    /// Gets the identifier of the user that attempted to delete themselves.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;
}
