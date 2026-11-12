using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application.Common.Paging;

/// <summary>
/// A single-field sort parsed from a query-string token: <c>name</c> and <c>+name</c> sort ascending,
/// <c>-name</c> descending. The field is trimmed and lowercased; each list endpoint validates it
/// against a per-aggregate whitelist.
/// </summary>
public readonly record struct Sort(string Field, SortDirection Direction) : IParsable<Sort>
{
    public static Sort Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid sort value: '{s}'.");
    }

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
