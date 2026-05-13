namespace Fargo.Core.Tokens;

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
/// <summary>
/// Defines the contract for hashing security tokens.
///
/// Implementations are responsible for producing a deterministic
/// hash of a token so that the system can safely store the hash
/// instead of the plaintext token.
/// </summary>
public interface ITokenHasher
{
    /// <summary>
    /// Generates a hash for the specified token.
    /// </summary>
    /// <param name="token">The plaintext token.</param>
    /// <returns>A <see cref="TokenHash"/> representing the hashed token.</returns>
    TokenHash Hash(Token token);
}
