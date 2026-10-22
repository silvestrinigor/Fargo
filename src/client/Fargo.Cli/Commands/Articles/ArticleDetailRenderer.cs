using Fargo.Cli.Output;
using Fargo.ClientHttp.Models;
using System.Globalization;

namespace Fargo.Cli.Commands.Articles;

/// <summary>
/// Renders the full details of an <see cref="ArticleDto"/> as a key/value block,
/// surfacing every field exposed by the API (barcodes, colour, dimensions, mass,
/// container, pack, variation, kit components, and partitions). Shared by the
/// <c>get</c> and <c>get-by-barcode</c> commands so both show the same detail.
/// </summary>
internal static class ArticleDetailRenderer
{
    public static void Write(ArticleDto article)
    {
        new DetailWriter()
            .Title("Article")
            .Field("GUID", article.Guid)
            .Field("Name", article.Name)
            .Field("Type", EnumFormatting.Format<ArticleType>(article.ArticleType))
            .Field("Description", article.Description)
            .Field("Shelf life", article.ShelfLife)
            .Field("Barcode (EAN-13)", article.Barcodes?.Ean13)
            .Field("Color", FormatColor(article.Color))
            .Field("Dimension", FormatDimension(article.Dimension))
            .Field("Mass", FormatMeasure(article.Mass?.Unit, article.Mass?.Value))
            .Field("Container max mass", FormatMeasure(article.Container?.MaxMass?.Unit, article.Container?.MaxMass?.Value))
            .Field("Pack source article", article.Pack?.FromArticleGuid)
            .Field("Pack quantity", FormatScalar(article.Pack?.Quantity))
            .Field("Variation source article", article.Variation?.FromArticleGuid)
            .Field("Kit components", FormatKitComponents(article.KitComponents))
            .Field("Partitions", FormatGuids(article.Partitions))
            .Write();
    }

    private static string? FormatGuids(IEnumerable<Guid?>? guids)
    {
        if (guids is null)
        {
            return null;
        }

        var values = guids.Where(g => g.HasValue).Select(g => g!.Value.ToString()).ToArray();

        return values.Length == 0 ? null : string.Join(", ", values);
    }

    private static string? FormatMeasure(string? unit, double? value)
    {
        if (value is null && string.IsNullOrEmpty(unit))
        {
            return null;
        }

        var number = value?.ToString(CultureInfo.InvariantCulture);

        return string.IsNullOrEmpty(unit)
            ? number
            : $"{number} {unit}".Trim();
    }

    private static string? FormatScalar(Scalar? scalar)
    {
        if (scalar is null)
        {
            return null;
        }

        if (scalar.Double is { } d)
        {
            return d.ToString(CultureInfo.InvariantCulture);
        }

        return scalar.String;
    }

    private static string? FormatDimension(ArticleDimensionDto? dimension)
    {
        if (dimension is null)
        {
            return null;
        }

        var x = FormatMeasure(dimension.LengthX?.Unit, dimension.LengthX?.Value);
        var y = FormatMeasure(dimension.LengthY?.Unit, dimension.LengthY?.Value);
        var z = FormatMeasure(dimension.LengthZ?.Unit, dimension.LengthZ?.Value);

        if (x is null && y is null && z is null)
        {
            return null;
        }

        return $"X: {x ?? "-"}, Y: {y ?? "-"}, Z: {z ?? "-"}";
    }

    private static string? FormatColor(Color? color)
    {
        if (color?.AdditionalData is not { Count: > 0 } data)
        {
            return null;
        }

        var parts = data.Select(kv => $"{kv.Key}: {kv.Value}");

        return string.Join(", ", parts);
    }

    private static string? FormatKitComponents(IEnumerable<ArticleKitComponentDto>? components)
    {
        if (components is null)
        {
            return null;
        }

        var parts = components
            .Select(c =>
            {
                var quantity = FormatScalar(c.Quantity);
                return quantity is null
                    ? $"{c.ArticleGuid}"
                    : $"{c.ArticleGuid} (x{quantity})";
            })
            .ToArray();

        return parts.Length == 0 ? null : string.Join(", ", parts);
    }
}
