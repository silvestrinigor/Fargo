using System.Globalization;

namespace Fargo.Cli.Commands.Articles;

/// <summary>
/// Parses compact command-line tokens for the article value-objects exposed by
/// the API: measures written as a single <c>value+unit</c> token (for example
/// <c>2.5kg</c> or <c>10 cm</c>) and kit components written as
/// <c>&lt;article-guid&gt;:&lt;quantity&gt;</c>.
/// </summary>
internal static class MeasureParser
{
    /// <summary>
    /// Splits a measure token such as <c>2.5kg</c>, <c>10cm</c>, or <c>10 cm</c>
    /// into its numeric value and its unit abbreviation. The numeric part may be
    /// negative and may use a decimal point. Everything after the number (trimmed)
    /// is treated as the unit.
    /// </summary>
    /// <param name="token">The raw token, e.g. <c>2.5kg</c>.</param>
    /// <param name="value">The parsed numeric value.</param>
    /// <param name="unit">The parsed unit abbreviation (e.g. <c>kg</c>).</param>
    /// <returns><see langword="true"/> when the token is a valid value+unit pair.</returns>
    public static bool TryParseMeasure(string? token, out double value, out string unit)
    {
        value = 0;
        unit = string.Empty;

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        token = token.Trim();

        // Find the boundary between the leading numeric part and the trailing unit.
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
    /// Parses a measure token, throwing a <see cref="FormatException"/> with a
    /// helpful message when the token is malformed. Used when the option has a
    /// value and a bad value should abort the command.
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
    /// Parses a kit-component token of the form
    /// <c>&lt;article-guid&gt;:&lt;quantity&gt;</c>. The quantity is optional; when
    /// omitted the token is just the GUID and the quantity is returned as
    /// <see langword="null"/>.
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
}
