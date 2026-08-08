using Fargo.Core.Shared.Identity;

namespace Fargo.Core.Identity;

/// <summary>
/// Defines the contract for generating refresh tokens used by the
/// authentication system.
///
/// Implementations are responsible for generating cryptographically secure
/// random tokens suitable for authentication workflows.
/// </summary>
public interface IRefreshTokenGenerator
{
    /// <summary>
    /// Generates a new refresh token.
    /// </summary>
    /// <returns>
    /// A newly generated <see cref="Token"/>.
    /// </returns>
    Token Generate();
}
