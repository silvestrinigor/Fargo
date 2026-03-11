namespace Fargo.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when an operation requires an active user group,
    /// but the specified group is inactive.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserGroupInactiveFargoDomainException"/> class.
    /// </remarks>
    /// <param name="userGroupGuid">
    /// The unique identifier of the inactive user group.
    /// </param>
    public sealed class UserGroupInactiveFargoDomainException(Guid userGroupGuid)
        : FargoDomainException($"The user group '{userGroupGuid}' is inactive.")
    {

        /// <summary>
        /// Gets the unique identifier of the inactive user group.
        /// </summary>
        public Guid UserGroupGuid
        {
            get;
        } = userGroupGuid;
    }
}