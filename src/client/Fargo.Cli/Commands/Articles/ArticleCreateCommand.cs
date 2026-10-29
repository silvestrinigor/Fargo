using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class ArticleCreateCommand : Command
{
    // Mirrors the server's ArticleType enum numeric values.
    private const int ArticleTypeDefault = 1;
    private const int ArticleTypeVariation = 2;
    private const int ArticleTypePack = 3;
    private const int ArticleTypeKit = 4;
    private const int ArticleTypeContainer = 5;

    // Mirrors the server's ArticleTrackingMode enum numeric values.
    private const int TrackingModeEntity = 1;
    private const int TrackingModeEvent = 2;

    public ArticleCreateCommand(FargoApiClient client)
        : base("create", """
            Create a new article.

            The article type is inferred from the options you pass; you do not set
            it explicitly:
              - variation : --variation-from <guid>
              - pack      : --pack-from <guid> and/or --pack-quantity <n>
              - kit       : one or more --kit-component
              - container : --is-container
              - default   : none of the above
            These are mutually exclusive: combining options that imply different
            types (for example --variation-from with --pack-from) is an error.

            Measures are written as a number followed by a unit, e.g. '2.5kg' or
            '10cm'. Units are standard abbreviations (mass: g, kg, mg, lb, oz;
            length: mm, cm, m, in, ft). Kit components are written as
            '<article-guid>' or '<article-guid>:<quantity>' and may be repeated.

            Examples:
              fargo article create --name "Widget"
              fargo article create --name "Widget" --mass 2.5kg --length-x 10cm --length-y 5cm --length-z 2cm
              fargo article create --name "Bundle" \
                --kit-component 11111111-1111-1111-1111-111111111111:2 \
                --kit-component 22222222-2222-2222-2222-222222222222
              fargo article create --name "Case" --is-container --container-max-mass 25kg
              fargo article create --name "6-pack" \
                --pack-from 11111111-1111-1111-1111-111111111111 --pack-quantity 6
              fargo article create --name "Blue Widget" \
                --variation-from 11111111-1111-1111-1111-111111111111
              fargo article create --name "Widget" --barcode-ean13 1234567890128
              fargo article create --name "Bulk sand" --tracking-mode event
            """)
    {
        var nameOption = new Option<string>("--name")
        {
            Description = "Name of the article.",
            Required = true
        };

        var descriptionOption = new Option<string?>("--description")
        {
            Description = "Optional description of the article."
        };

        var shelfLifeOption = new Option<string?>("--shelf-life")
        {
            Description = "Optional shelf life (e.g. an ISO-8601 duration like 'P30D')."
        };

        var colorOption = new Option<string?>("--color")
        {
            Description = "Optional color of the article (e.g. a hex code like '#FF0000')."
        };

        var massOption = new Option<string?>("--mass")
        {
            Description = "Optional mass as value+unit, e.g. '2.5kg'."
        };

        var lengthXOption = new Option<string?>("--length-x")
        {
            Description = "Optional dimension along X as value+unit, e.g. '10cm'."
        };

        var lengthYOption = new Option<string?>("--length-y")
        {
            Description = "Optional dimension along Y as value+unit, e.g. '5cm'."
        };

        var lengthZOption = new Option<string?>("--length-z")
        {
            Description = "Optional dimension along Z as value+unit, e.g. '2cm'."
        };

        var variationFromOption = new Option<Guid?>("--variation-from")
        {
            Description =
                "GUID of the source article this variation is created from. "
                + "Implies the article is a variation."
        };

        var packFromOption = new Option<Guid?>("--pack-from")
        {
            Description =
                "GUID of the source article this pack is composed of. "
                + "Implies the article is a pack."
        };

        var packQuantityOption = new Option<double?>("--pack-quantity")
        {
            Description =
                "Quantity of the source article contained in the pack. "
                + "Implies the article is a pack."
        };

        var isContainerOption = new Option<bool>("--is-container")
        {
            Description = "Create the article as a container."
        };

        var containerMaxMassOption = new Option<string?>("--container-max-mass")
        {
            Description =
                "Maximum mass the container can hold, as value+unit, e.g. '25kg'. "
                + "Implies the article is a container."
        };

        var barcodeEan13Option = new Option<string?>("--barcode-ean13")
        {
            Description = "Optional EAN-13 barcode to assign to the article."
        };

        var trackingModeOption = new Option<string?>("--tracking-mode")
        {
            Description =
                "How the article's stock is tracked: 'entity' (each unit is an "
                + "individual item; the default) or 'event' (aggregate quantities "
                + "recorded as movement events). Container articles are always "
                + "entity-tracked."
        };

        var kitComponentOption = new Option<string[]?>("--kit-component")
        {
            Description =
                "Kit component as '<article-guid>' or '<article-guid>:<quantity>'. "
                + "Repeat the option for multiple components. Implies the article is a kit.",
            AllowMultipleArgumentsPerToken = true
        };

        var partitionsOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Partition GUIDs to associate the article with (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(nameOption);
        Add(descriptionOption);
        Add(shelfLifeOption);
        Add(colorOption);
        Add(massOption);
        Add(lengthXOption);
        Add(lengthYOption);
        Add(lengthZOption);
        Add(variationFromOption);
        Add(packFromOption);
        Add(packQuantityOption);
        Add(isContainerOption);
        Add(containerMaxMassOption);
        Add(barcodeEan13Option);
        Add(trackingModeOption);
        Add(kitComponentOption);
        Add(partitionsOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var name = parseResult.GetValue(nameOption)!;
            var description = parseResult.GetValue(descriptionOption);
            var shelfLife = parseResult.GetValue(shelfLifeOption);
            var color = parseResult.GetValue(colorOption);
            var mass = parseResult.GetValue(massOption);
            var lengthX = parseResult.GetValue(lengthXOption);
            var lengthY = parseResult.GetValue(lengthYOption);
            var lengthZ = parseResult.GetValue(lengthZOption);
            var variationFrom = parseResult.GetValue(variationFromOption);
            var packFrom = parseResult.GetValue(packFromOption);
            var packQuantity = parseResult.GetValue(packQuantityOption);
            var isContainer = parseResult.GetValue(isContainerOption);
            var containerMaxMass = parseResult.GetValue(containerMaxMassOption);
            var barcodeEan13 = parseResult.GetValue(barcodeEan13Option);
            var trackingModeRaw = parseResult.GetValue(trackingModeOption);
            var kitComponents = parseResult.GetValue(kitComponentOption);
            var partitions = parseResult.GetValue(partitionsOption);

            // Infer the article type from the options the user supplied, and
            // reject combinations that imply more than one type.
            int articleType;
            try
            {
                articleType = InferArticleType(
                    variationPresent: variationFrom is not null,
                    packPresent: packFrom is not null || packQuantity is not null,
                    kitPresent: kitComponents is { Length: > 0 },
                    containerPresent: isContainer || containerMaxMass is not null);
            }
            catch (FormatException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }

            // Resolve the tracking mode (defaults to entity). Event tracking is
            // only meaningful for non-container articles; containers are always
            // entity-tracked, so reject the combination explicitly.
            int trackingMode;
            try
            {
                trackingMode = ParseTrackingMode(trackingModeRaw);
            }
            catch (FormatException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }

            if (trackingMode == TrackingModeEvent && articleType == ArticleTypeContainer)
            {
                Console.Error.WriteLine(
                    "Container articles are always entity-tracked; "
                    + "--tracking-mode event cannot be combined with a container.");
                return 1;
            }

            var dto = new ArticleCreateDto
            {
                Name = name,
                ArticleType = articleType,
                TrackingMode = trackingMode,
                Description = description,
                ShelfLife = shelfLife,
                Color = color,
                PartitionsToAdd = partitions?.Select(g => (Guid?)g).ToList()
            };

            try
            {
                if (mass is not null)
                {
                    var (value, unit) = MeasureParser.ParseMeasure(mass, "--mass");
                    dto.Mass = new ArticleCreateDto_mass { Value = value, Unit = unit };
                }

                var dimension = BuildDimension(lengthX, lengthY, lengthZ);
                if (dimension is not null)
                {
                    dto.Dimension = dimension;
                }

                if (variationFrom is not null)
                {
                    dto.Variation = new ArticleVariationDto { FromArticleGuid = variationFrom };
                }

                if (packFrom is not null || packQuantity is not null)
                {
                    dto.Pack = new ArticlePackDto
                    {
                        FromArticleGuid = packFrom,
                        Quantity = packQuantity is { } q ? new Scalar { Double = q } : null
                    };
                }

                if (containerMaxMass is not null)
                {
                    var (value, unit) = MeasureParser.ParseMeasure(containerMaxMass, "--container-max-mass");
                    dto.Container = new ArticleContainerDto
                    {
                        MaxMass = new ArticleContainerDto_maxMass { Value = value, Unit = unit }
                    };
                }

                if (!string.IsNullOrWhiteSpace(barcodeEan13))
                {
                    dto.Barcode = new ArticleBarcodeDto { Ean13 = barcodeEan13 };
                }

                if (kitComponents is { Length: > 0 })
                {
                    dto.KitComponents = kitComponents
                        .Select(token =>
                        {
                            var (articleGuid, quantity) = MeasureParser.ParseKitComponent(token);
                            return new ArticleKitComponentDto
                            {
                                ArticleGuid = articleGuid,
                                Quantity = quantity is { } q ? new Scalar { Double = q } : null
                            };
                        })
                        .ToList();
                }
            }
            catch (FormatException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }

            var guid = await client.V1.Articles.PostAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success(
                "Article created.",
                ("GUID", guid?.ToString()),
                ("Name", name));

            return 0;
        });
    }

    /// <summary>
    /// Determines the article type from which groups of options were supplied.
    /// Exactly one type-signalling group may be present; when none are present
    /// the article is a plain default article. Throws a
    /// <see cref="FormatException"/> when the supplied options imply more than
    /// one article type.
    /// </summary>
    private static int InferArticleType(
        bool variationPresent,
        bool packPresent,
        bool kitPresent,
        bool containerPresent)
    {
        var signals = new List<(bool Present, int Type, string OptionHint)>
        {
            (variationPresent, ArticleTypeVariation, "--variation-from"),
            (packPresent, ArticleTypePack, "--pack-from / --pack-quantity"),
            (kitPresent, ArticleTypeKit, "--kit-component"),
            (containerPresent, ArticleTypeContainer, "--is-container / --container-max-mass"),
        };

        var present = signals.Where(s => s.Present).ToList();

        if (present.Count > 1)
        {
            var hints = string.Join(", ", present.Select(s => s.OptionHint));
            throw new FormatException(
                $"Conflicting article type options: {hints}. "
                + "An article can only be one of variation, pack, kit, or container. "
                + "Supply the options for a single type.");
        }

        return present.Count == 1 ? present[0].Type : ArticleTypeDefault;
    }

    /// <summary>
    /// Parses the <c>--tracking-mode</c> option. Accepts 'entity' or 'event'
    /// (case-insensitive); when omitted, defaults to entity tracking. Throws a
    /// <see cref="FormatException"/> for any other value.
    /// </summary>
    private static int ParseTrackingMode(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return TrackingModeEntity;
        }

        return raw.Trim().ToLowerInvariant() switch
        {
            "entity" => TrackingModeEntity,
            "event" => TrackingModeEvent,
            _ => throw new FormatException(
                $"Invalid --tracking-mode '{raw}'. Expected 'entity' or 'event'."),
        };
    }

    private static ArticleDimensionDto? BuildDimension(string? lengthX, string? lengthY, string? lengthZ)
    {
        if (lengthX is null && lengthY is null && lengthZ is null)
        {
            return null;
        }

        var dimension = new ArticleDimensionDto();

        if (lengthX is not null)
        {
            var (value, unit) = MeasureParser.ParseMeasure(lengthX, "--length-x");
            dimension.LengthX = new ArticleDimensionDto_lengthX { Value = value, Unit = unit };
        }

        if (lengthY is not null)
        {
            var (value, unit) = MeasureParser.ParseMeasure(lengthY, "--length-y");
            dimension.LengthY = new ArticleDimensionDto_lengthY { Value = value, Unit = unit };
        }

        if (lengthZ is not null)
        {
            var (value, unit) = MeasureParser.ParseMeasure(lengthZ, "--length-z");
            dimension.LengthZ = new ArticleDimensionDto_lengthZ { Value = value, Unit = unit };
        }

        return dimension;
    }
}
