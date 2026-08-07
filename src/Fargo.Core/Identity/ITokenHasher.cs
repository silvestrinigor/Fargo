using Fargo.Core.Shared.Identity;

namespace Fargo.Core.Identity;

/// <summary>
/// Defines the contract for hashing sensitive token values.
///
/// Implementations are responsible for producing a deterministic hash of a
/// token so that the system can store the hash instead of the original value.
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
