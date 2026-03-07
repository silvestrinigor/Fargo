using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Security
{
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
}