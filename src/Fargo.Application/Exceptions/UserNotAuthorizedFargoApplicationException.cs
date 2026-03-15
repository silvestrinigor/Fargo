using Fargo.Domain.Enums;

namespace Fargo.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a user attempts to perform an action
    /// for which they do not have permission.
    /// </summary>
    public sealed class UserNotAuthorizedFargoApplicationException(
            Guid userGuid,
            ActionType actionType
            ) : FargoApplicationException(
                $"User '{userGuid}' is not authorized to perform action '{actionType}'.")
    {
        /// <summary>
        /// Gets the identifier of the user that attempted the action.
        /// </summary>
        public Guid UserGuid { get; } = userGuid;

        /// <summary>
        /// Gets the action the user attempted to perform.
        /// </summary>
        public ActionType ActionType { get; } = actionType;
    }
}