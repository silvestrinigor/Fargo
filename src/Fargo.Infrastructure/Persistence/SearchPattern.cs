namespace Fargo.Infrastructure.Persistence;

/// <summary>
/// Escapes the LIKE wildcards (<c>%</c> and <c>_</c>) and the escape character itself so user search text
/// is matched literally. Callers wrap the escaped term in their own <c>%...%</c> and pass
/// <see cref="EscapeChar"/> as the ESCAPE clause of <c>ILike</c>.
/// </summary>
internal static class SearchPattern
{
    public const string EscapeChar = "\\";

    public static string Escape(string term) => term
        .Replace("\\", "\\\\")
        .Replace("%", "\\%")
        .Replace("_", "\\_");
}
