using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to create a <see cref="Entities.UserGroup"/>
    /// with a <see cref="Nameid"/> that already exists in the system.
    /// </summary>
    public sealed class UserGroupNameidAlreadyExistsDomainException(
            Nameid nameid
            ) : FargoDomainException(
                $"A user group with nameid '{nameid}' already exists."
                )
    {
        /// <summary>
        /// Gets the conflicting <see cref="Nameid"/>.
        /// </summary>
        public Nameid Nameid { get; } = nameid;
    }
}