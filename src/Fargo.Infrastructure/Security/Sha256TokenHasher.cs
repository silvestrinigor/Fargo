using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;
using System.Security.Cryptography;
using System.Text;

namespace Fargo.Infrastructure.Security;

/// <summary>
/// Hashes tokens using the SHA-256 cryptographic hash algorithm.
/// </summary>
/// <remarks>
/// This implementation computes the SHA-256 hash of the provided token
/// and encodes the resulting bytes as an uppercase hexadecimal string.
///
/// The resulting <see cref="TokenHash"/> can be safely stored in persistent
/// storage without exposing the original token value.
/// </remarks>
public class Sha256TokenHasher : ITokenHasher
{
    /// <summary>
    /// Computes the SHA-256 hash of the specified token.
    /// </summary>
    /// <param name="token">
    /// The token whose value will be hashed.
    /// </param>
    /// <returns>
    /// A <see cref="TokenHash"/> containing the hexadecimal SHA-256 hash
    /// of the provided token.
    /// </returns>
    public TokenHash Hash(Token token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));

        var tokenHexStringHash = Convert.ToHexString(bytes);

        return new TokenHash(tokenHexStringHash);
    }
}
