namespace Fargo.Mcp.Hosting;

internal static class BearerToken
{
    private const string Scheme = "Bearer ";

    /// <summary>
    /// Returns null when the header is missing or is not a bearer token.
    /// </summary>
    public static string? Parse(string? authorizationHeader)
    {
        if (authorizationHeader is null
            || !authorizationHeader.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token = authorizationHeader[Scheme.Length..].Trim();

        return token.Length == 0 ? null : token;
    }
}
