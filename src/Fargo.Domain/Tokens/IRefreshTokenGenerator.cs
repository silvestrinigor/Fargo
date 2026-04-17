namespace Fargo.Domain.Tokens;

/// <summary>
/// Defines the contract for generating refresh tokens used
/// in the authentication system.
///
/// Implementations are responsible for producing secure,
/// cryptographically random tokens that can later be hashed
/// and stored by the system.
/// </summary>
public interface IRefreshTokenGenerator
{
    /// <summary>
    /// Generates a new refresh token.
    /// </summary>
    /// <returns>A newly generated <see cref="Token"/>.</returns>
    Token Generate();
}
