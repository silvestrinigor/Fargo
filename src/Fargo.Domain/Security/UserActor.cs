using Fargo.Domain.Entities;

namespace Fargo.Domain.Security;

/// <summary>
/// Represents an actor corresponding to a real authenticated <see cref="User"/>.
/// </summary>
/// <remarks>
/// This actor is used when an operation is initiated by a real user.
/// </remarks>
public sealed class UserActor : IActor
{
    /// <summary>
    /// Gets the unique identifier of the actor.
    /// </summary>
    public Guid Guid => User.Guid;

    /// <summary>
    /// Gets the associated <see cref="User"/>.
    /// </summary>
    public User User { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="UserActor"/>.
    /// </summary>
    /// <param name="user">
    /// The user associated with the actor.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="user"/> is null.
    /// </exception>
    public UserActor(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        User = user;
    }
}
