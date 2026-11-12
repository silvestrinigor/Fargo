using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Identity.Credentials;
using System.Security.Cryptography;

namespace Fargo.Infrastructure.Security;

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
