using System.Security.Cryptography;
using System.Text;

namespace Fargo.Domain.ClientApplications;

/// <summary>
/// Generates and hashes API keys for <see cref="ClientApplication"/> entities.
/// </summary>
public static class ApiKeyGenerator
{
    private const string Prefix = "fargo_";

    /// <summary>
    /// Generates a new API key in the format <c>fargo_&lt;base64url&gt;</c> (32 bytes of entropy).
    /// </summary>
    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        var encoded = Convert.ToBase64String(bytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        return Prefix + encoded;
    }

    /// <summary>
    /// Returns the uppercase SHA-256 hex hash of <paramref name="apiKey"/> used for storage and lookup.
    /// </summary>
    public static string Hash(string apiKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));

        return Convert.ToHexString(bytes);
    }
}
