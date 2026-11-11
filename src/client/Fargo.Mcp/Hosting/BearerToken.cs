namespace Fargo.Mcp.Hosting;

internal static class BearerToken
{
    private const string Scheme = "Bearer ";

    /// <summary>
    /// Extracts the token from an <c>Authorization: Bearer &lt;token&gt;</c> header
    /// value, or returns null when the header is missing or not a bearer token.
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
