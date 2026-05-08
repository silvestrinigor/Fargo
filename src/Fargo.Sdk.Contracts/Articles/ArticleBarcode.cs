using System.Diagnostics.CodeAnalysis;

namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents an article barcode route value in the format <c>{barcode}:{type}</c>.</summary>
public readonly record struct ArticleBarcode(string Barcode, ArticleBarcodeType Type)
    : IParsable<ArticleBarcode>
{
    public override string ToString() => $"{Barcode}:{Type}";

    public static ArticleBarcode Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid article barcode value: '{s}'. Expected '{{barcode}}:{{type}}'.");
    }

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out ArticleBarcode result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        var separator = s.LastIndexOf(':');
        if (separator <= 0 || separator == s.Length - 1)
        {
            return false;
        }

        var barcode = s[..separator];
        var typeText = s[(separator + 1)..];

        if (string.IsNullOrWhiteSpace(barcode) ||
            !Enum.TryParse<ArticleBarcodeType>(typeText, ignoreCase: true, out var type))
        {
            return false;
        }

        result = new ArticleBarcode(barcode, type);
        return true;
    }
}
