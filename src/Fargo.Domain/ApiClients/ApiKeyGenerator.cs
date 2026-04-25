using System.Security.Cryptography;
using System.Text;

namespace Fargo.Domain.ApiClients;

public static class ApiKeyGenerator
{
    private const string Prefix = "fargo_";

    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var encoded = Convert.ToBase64String(bytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        return Prefix + encoded;
    }

    public static string Hash(string apiKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToHexString(bytes);
    }
}
