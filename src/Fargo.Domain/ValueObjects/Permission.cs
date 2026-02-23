using Fargo.Domain.Enums;

namespace Fargo.Domain.ValueObjects
{
    /// <summary>
    /// Represents a permission with an action type and grant type.
    /// </summary>
    public readonly struct Permission
    {
        /// <summary>
        /// Gets or initializes the type of action that this permission grants.
        /// </summary>
        public required ActionType ActionType
        {
            get;
            init;
        }

        /// <summary>
        /// Gets or initializes the type of grant for this permission.
        /// </summary>
        public required GrantType GrantType
        {
            get;
            init;
        }
    }
}