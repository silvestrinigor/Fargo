namespace Fargo.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a user attempts to change their own permissions.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserCannotChangeOwnPermissionsFargoDomainException"/> class.
    /// </remarks>
    /// <param name="userGuid">
    /// The identifier of the user who attempted to change their own permissions.
    /// </param>
    public sealed class UserCannotChangeOwnPermissionsFargoDomainException(Guid userGuid)
                : FargoDomainException($"User '{userGuid}' cannot change their own permissions.")
    {
        /// <summary>
        /// Gets the identifier of the user who attempted to change their own permissions.
        /// </summary>
        public Guid UserGuid { get; } = userGuid;
    }
}