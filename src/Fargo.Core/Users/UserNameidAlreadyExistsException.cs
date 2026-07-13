using Fargo.Core.Shared;

namespace Fargo.Core.Users;

/// <summary>
/// Represents an error that occurs when attempting to set
/// a <c>User</c> with a <see cref="Nameid"/> that already exists.
/// </summary>
public sealed class UserNameidAlreadyExistsDomainException(Nameid nameid)
    : FargoException($"A user with Nameid '{nameid}' already exists.")
{
    /// <summary>
    /// Gets the <see cref="Nameid"/> that caused the conflict.
    /// </summary>
    public Nameid Nameid { get; } = nameid;
}
