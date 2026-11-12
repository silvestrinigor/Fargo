using System.Globalization;

namespace Fargo.Cli.Commands.Articles;

/// <summary>
/// Parses compact command-line tokens for article values: measures such as <c>2.5kg</c> or
/// <c>10 cm</c>, kit components and container allowed articles.
/// </summary>
internal static class MeasureParser
{
    /// <summary>
    /// Splits a token such as <c>2.5kg</c> or <c>10 cm</c> into a number and a unit. Everything
    /// after the number (trimmed) is the unit; a number without a unit is not valid.
    /// </summary>
    public static bool TryParseMeasure(string? token, out double value, out string unit)
    {
        value = 0;
        unit = string.Empty;

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        token = token.Trim();

        var index = 0;
        while (index < token.Length)
        {
            var c = token[index];
            var isNumeric = char.IsDigit(c)
                || c == '.'
                || c == '-'
                || c == '+'
                || c == ',';

            if (!isNumeric)
            {
                break;
            }

            index++;
        }

        if (index == 0)
        {
            return false;
        }

        var numberPart = token[..index];
        var unitPart = token[index..].Trim();

        if (unitPart.Length == 0)
        {
            return false;
        }

        if (!double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            return false;
        }

        unit = unitPart;
        return true;
    }

    /// <summary>
    /// Throws a <see cref="FormatException"/> when the token is malformed, so a bad value aborts
    /// the command.
    /// </summary>
    public static (double Value, string Unit) ParseMeasure(string token, string optionName)
    {
        if (!TryParseMeasure(token, out var value, out var unit))
        {
            throw new FormatException(
                $"Invalid value for {optionName}: '{token}'. "
                + "Expected a number followed by a unit, e.g. '2.5kg' or '10 cm'.");
        }

        return (value, unit);
    }

    /// <summary>
    /// Parses <c>&lt;article-guid&gt;:&lt;quantity&gt;</c>. The quantity is optional and is
    /// <see langword="null"/> when omitted.
    /// </summary>
    public static (Guid ArticleGuid, double? Quantity) ParseKitComponent(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new FormatException("A kit component cannot be empty.");
        }

        token = token.Trim();

        var separator = token.LastIndexOf(':');

        string guidPart;
        double? quantity = null;

        if (separator < 0)
        {
            guidPart = token;
        }
        else
        {
            guidPart = token[..separator].Trim();
            var quantityPart = token[(separator + 1)..].Trim();

            if (quantityPart.Length > 0)
            {
                if (!double.TryParse(quantityPart, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                {
                    throw new FormatException(
                        $"Invalid kit component quantity '{quantityPart}' in '{token}'. "
                        + "Expected a number, e.g. '{guid}:3'.");
                }

                quantity = parsed;
            }
        }

        if (!Guid.TryParse(guidPart, out var articleGuid))
        {
            throw new FormatException(
                $"Invalid kit component '{token}'. "
                + "Expected '<article-guid>' or '<article-guid>:<quantity>'.");
        }

        return (articleGuid, quantity);
    }

    /// <summary>
    /// Parses <c>&lt;article-guid&gt;</c> or <c>&lt;article-guid&gt;:&lt;max-quantity&gt;</c>.
    /// Without a maximum quantity the article is allowed with no cap (<see langword="null"/>);
    /// when present it must be greater than zero.
    /// </summary>
    public static (Guid ArticleGuid, double? MaxQuantity) ParseAllowedArticle(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new FormatException("An allowed article cannot be empty.");
        }

        token = token.Trim();

        var separator = token.LastIndexOf(':');

        string guidPart;
        double? maxQuantity = null;

        if (separator < 0)
        {
            guidPart = token;
        }
        else
        {
            guidPart = token[..separator].Trim();
            var quantityPart = token[(separator + 1)..].Trim();

            if (quantityPart.Length > 0)
            {
                if (!double.TryParse(quantityPart, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                {
                    throw new FormatException(
                        $"Invalid allowed article maximum quantity '{quantityPart}' in '{token}'. "
                        + "Expected a number, e.g. '{guid}:10'.");
                }

                if (parsed <= 0)
                {
                    throw new FormatException(
                        $"The allowed article maximum quantity in '{token}' must be greater than zero.");
                }

                maxQuantity = parsed;
            }
        }

        if (!Guid.TryParse(guidPart, out var articleGuid))
        {
            throw new FormatException(
                $"Invalid allowed article '{token}'. "
                + "Expected '<article-guid>' or '<article-guid>:<max-quantity>'.");
        }

        return (articleGuid, maxQuantity);
    }
}
