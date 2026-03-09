using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Exceptions
{
    /// <summary>
    /// Represents an error that occurs when attempting to create
    /// a <c>User</c> with a <see cref="Nameid"/> that already exists.
    ///
    /// In the domain, a <see cref="Nameid"/> must be unique
    /// across all users.
    /// </summary>
    public sealed class UserNameidAlreadyExistsDomainException(Nameid nameid)
        : FargoDomainException($"A user with Nameid '{nameid}' already exists.")
    {
        /// <summary>
        /// Gets the <see cref="Nameid"/> that caused the conflict.
        /// </summary>
        public Nameid Nameid { get; } = nameid;
    }
}