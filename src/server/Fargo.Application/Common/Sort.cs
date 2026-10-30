using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application.Common;

/// <summary>
/// A single-field sort specification for a list query. Parsed from a query-string
/// token where a leading '-' means descending: <c>name</c> sorts ascending by the
/// field "name", <c>-name</c> sorts descending. The field name is lowercased and
/// trimmed; each list endpoint validates it against a per-aggregate whitelist.
/// </summary>
/// <remarks>
/// Implements <see cref="IParsable{TSelf}"/> so ASP.NET can bind it directly from
/// the query string, exactly like <see cref="Page"/> and <see cref="Limit"/>.
/// </remarks>
public readonly record struct Sort(string Field, SortDirection Direction) : IParsable<Sort>
{
    /// <inheritdoc />
    public static Sort Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid sort value: '{s}'.");
    }

    /// <inheritdoc />
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out Sort result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        var token = s.Trim();

        var direction = SortDirection.Ascending;

        if (token.StartsWith('-'))
        {
            direction = SortDirection.Descending;
            token = token[1..];
        }
        else if (token.StartsWith('+'))
        {
            token = token[1..];
        }

        token = token.Trim();

        if (token.Length == 0)
        {
            return false;
        }

        result = new Sort(token.ToLowerInvariant(), direction);
        return true;
    }
}