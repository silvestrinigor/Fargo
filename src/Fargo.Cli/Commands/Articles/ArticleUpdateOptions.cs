using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

/// <summary>
/// Declares the full set of options that map onto <see cref="ArticleUpdateDto"/>
/// and builds the DTO from a parse result. Shared by the GUID-addressed
/// <c>update</c> command and the barcode-addressed <c>update-by-barcode</c>
/// command so both expose an identical surface.
/// </summary>
internal sealed class ArticleUpdateOptions
{
    public Option<string?> Name { get; } = new("--name")
    {
        Description = "New name for the article."
    };

    public Option<string?> Description { get; } = new("--description")
    {
        Description = "New description. Leaves the existing value unchanged when omitted."
    };

    public Option<string?> ShelfLife { get; } = new("--shelf-life")
    {
        Description = "New shelf life (e.g. 'P30D'). Leaves the existing value unchanged when omitted."
    };

    public Option<bool> RemoveShelfLife { get; } = new("--remove-shelf-life")
    {
        Description = "Clear the existing shelf life. Takes precedence over --shelf-life."
    };

    public Option<string?> Mass { get; } = new("--mass")
    {
        Description = "New mass as value+unit, e.g. '2.5kg'. Leaves the existing value unchanged when omitted."
    };

    public Option<bool> RemoveMass { get; } = new("--remove-mass")
    {
        Description = "Clear the existing mass. Takes precedence over --mass."
    };

    public Option<string?> LengthX { get; } = new("--length-x")
    {
        Description = "New dimension along X as value+unit, e.g. '10cm'."
    };

    public Option<string?> LengthY { get; } = new("--length-y")
    {
        Description = "New dimension along Y as value+unit, e.g. '5cm'."
    };

    public Option<string?> LengthZ { get; } = new("--length-z")
    {
        Description = "New dimension along Z as value+unit, e.g. '2cm'."
    };

    public Option<bool> RemoveLengthX { get; } = new("--remove-length-x")
    {
        Description = "Clear the existing X dimension. Takes precedence over --length-x."
    };

    public Option<bool> RemoveLengthY { get; } = new("--remove-length-y")
    {
        Description = "Clear the existing Y dimension. Takes precedence over --length-y."
    };

    public Option<bool> RemoveLengthZ { get; } = new("--remove-length-z")
    {
        Description = "Clear the existing Z dimension. Takes precedence over --length-z."
    };

    public Option<string?> BarcodeEan13 { get; } = new("--barcode-ean13")
    {
        Description = "New EAN-13 barcode. Leaves the existing barcode unchanged when omitted."
    };

    public Option<bool> RemoveBarcode { get; } = new("--remove-barcode")
    {
        Description = "Clear the existing EAN-13 barcode. Takes precedence over --barcode-ean13."
    };

    public Option<Guid[]?> AddPartitions { get; } = new("--add-partitions")
    {
        Description = "Partition GUIDs to add (space-separated).",
        AllowMultipleArgumentsPerToken = true
    };

    public Option<Guid[]?> RemovePartitions { get; } = new("--remove-partitions")
    {
        Description = "Partition GUIDs to remove (space-separated).",
        AllowMultipleArgumentsPerToken = true
    };

    /// <summary>Adds every update option to the given command.</summary>
    public void AddTo(Command command)
    {
        command.Add(Name);
        command.Add(Description);
        command.Add(ShelfLife);
        command.Add(RemoveShelfLife);
        command.Add(Mass);
        command.Add(RemoveMass);
        command.Add(LengthX);
        command.Add(LengthY);
        command.Add(LengthZ);
        command.Add(RemoveLengthX);
        command.Add(RemoveLengthY);
        command.Add(RemoveLengthZ);
        command.Add(BarcodeEan13);
        command.Add(RemoveBarcode);
        command.Add(AddPartitions);
        command.Add(RemovePartitions);
    }

    /// <summary>
    /// Builds the <see cref="ArticleUpdateDto"/> from the parsed options.
    /// Throws <see cref="FormatException"/> when a measure option is malformed.
    /// </summary>
    public ArticleUpdateDto Build(ParseResult parseResult)
    {
        var removeShelfLife = parseResult.GetValue(RemoveShelfLife);
        var removeMass = parseResult.GetValue(RemoveMass);

        var dto = new ArticleUpdateDto
        {
            Name = parseResult.GetValue(Name),
            Description = parseResult.GetValue(Description),
            ShelfLife = parseResult.GetValue(ShelfLife),
            RemoveShelfLife = removeShelfLife ? true : null,
            RemoveMass = removeMass ? true : null,
            PartitionsToAdd = parseResult.GetValue(AddPartitions)?.Select(g => (Guid?)g).ToList(),
            PartitionsToRemove = parseResult.GetValue(RemovePartitions)?.Select(g => (Guid?)g).ToList()
        };

        var mass = parseResult.GetValue(Mass);
        if (mass is not null)
        {
            var (value, unit) = MeasureParser.ParseMeasure(mass, "--mass");
            dto.Mass = new ArticleUpdateDto_mass { Value = value, Unit = unit };
        }

        var dimension = BuildDimension(parseResult);
        if (dimension is not null)
        {
            dto.Dimension = dimension;
        }

        var barcode = BuildBarcode(parseResult);
        if (barcode is not null)
        {
            dto.Barcode = barcode;
        }

        return dto;
    }

    private ArticleDimensionUpdateDto? BuildDimension(ParseResult parseResult)
    {
        var lengthX = parseResult.GetValue(LengthX);
        var lengthY = parseResult.GetValue(LengthY);
        var lengthZ = parseResult.GetValue(LengthZ);
        var removeX = parseResult.GetValue(RemoveLengthX);
        var removeY = parseResult.GetValue(RemoveLengthY);
        var removeZ = parseResult.GetValue(RemoveLengthZ);

        if (lengthX is null && lengthY is null && lengthZ is null
            && !removeX && !removeY && !removeZ)
        {
            return null;
        }

        var dimension = new ArticleDimensionUpdateDto
        {
            RemoveLengthX = removeX ? true : null,
            RemoveLengthY = removeY ? true : null,
            RemoveLengthZ = removeZ ? true : null
        };

        if (lengthX is not null)
        {
            var (value, unit) = MeasureParser.ParseMeasure(lengthX, "--length-x");
            dimension.LengthX = new ArticleDimensionUpdateDto_lengthX { Value = value, Unit = unit };
        }

        if (lengthY is not null)
        {
            var (value, unit) = MeasureParser.ParseMeasure(lengthY, "--length-y");
            dimension.LengthY = new ArticleDimensionUpdateDto_lengthY { Value = value, Unit = unit };
        }

        if (lengthZ is not null)
        {
            var (value, unit) = MeasureParser.ParseMeasure(lengthZ, "--length-z");
            dimension.LengthZ = new ArticleDimensionUpdateDto_lengthZ { Value = value, Unit = unit };
        }

        return dimension;
    }

    private ArticleBarcodeUpdateDto? BuildBarcode(ParseResult parseResult)
    {
        var ean13 = parseResult.GetValue(BarcodeEan13);
        var removeBarcode = parseResult.GetValue(RemoveBarcode);

        if (string.IsNullOrWhiteSpace(ean13) && !removeBarcode)
        {
            return null;
        }

        return new ArticleBarcodeUpdateDto
        {
            Ean13 = string.IsNullOrWhiteSpace(ean13) ? null : ean13,
            RemoveEan13 = removeBarcode ? true : null
        };
    }
}
