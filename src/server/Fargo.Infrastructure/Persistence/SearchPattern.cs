namespace Fargo.Infrastructure.Persistence;

/// <summary>
/// Helpers for building safe PostgreSQL LIKE/ILIKE patterns from user-supplied
/// search text. Escapes the LIKE wildcards ('%' and '_') and the escape
/// character itself so the search term is matched literally; callers wrap the
/// escaped term in their own '%...%' and pass <see cref="EscapeChar"/> as the
/// ESCAPE clause to EF's ILike.
/// </summary>
internal static class SearchPattern
{
    /// <summary>The ESCAPE character to pass to <c>EF.Functions.ILike</c>.</summary>
    public const string EscapeChar = "\\";

    /// <summary>Escapes LIKE wildcards in <paramref name="term"/> so they match literally.</summary>
    public static string Escape(string term) => term
        .Replace("\\", "\\\\")
        .Replace("%", "\\%")
        .Replace("_", "\\_");
}