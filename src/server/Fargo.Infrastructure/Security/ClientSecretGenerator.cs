using Fargo.Core.Identity;
using Fargo.Core.ServiceAccounts;
using Fargo.Core.Security;
using System.Security.Cryptography;

namespace Fargo.Infrastructure.Security;

/// <summary>
/// Generates cryptographically strong client secrets that satisfy the
/// <see cref="Password"/> policy (at least one letter, one digit and one special
/// character, no spaces, length within bounds).
/// </summary>
public sealed class ClientSecretGenerator : IClientSecretGenerator
{
    public Password Generate()
    {
        // 48 random bytes -> URL-safe base64 (no spaces). Append a fixed suffix
        // that guarantees the letter/digit/special requirements regardless of the
        // random portion, so the result always satisfies the Password policy.
        var random = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .Replace('+', 'A')
            .Replace('/', 'B')
            .Replace('=', 'C');

        var secret = random + "aA1!";

        return new Password(secret);
    }
}