using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class ArticleCreateCommand : Command
{
    public ArticleCreateCommand(FargoApiClient client)
        : base("create", """
            Create a new article.

            Measures are written as a number followed by a unit, e.g. '2.5kg' or
            '10cm'. Units are standard abbreviations (mass: g, kg, mg, lb, oz;
            length: mm, cm, m, in, ft). Kit components are written as
            '<article-guid>' or '<article-guid>:<quantity>' and may be repeated.

            Examples:
              fargo article create --name "Widget"
              fargo article create --name "Widget" --type pack --partitions 00000000-0000-0000-0000-000000000001
              fargo article create --name "Widget" --mass 2.5kg --length-x 10cm --length-y 5cm --length-z 2cm
              fargo article create --name "Bundle" --type kit \
                --kit-component 11111111-1111-1111-1111-111111111111:2 \
                --kit-component 22222222-2222-2222-2222-222222222222
              fargo article create --name "Case" --type container --container-max-mass 25kg
              fargo article create --name "6-pack" --type pack \
                --pack-from 11111111-1111-1111-1111-111111111111 --pack-quantity 6
              fargo article create --name "Widget" --barcode-ean13 1234567890128
            """)
    {
        var nameOption = new Option<string>("--name")
        {
            Description = "Name of the article.",
            Required = true
        };

        var typeOption = new Option<ArticleType>("--type")
        {
            Description =
                "Article type by name (default, variation, pack, kit, container). "
                + "Defaults to default.",
            DefaultValueFactory = _ => ArticleType.Default
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
            Description = "GUID of the source article this variation is created from."
        };

        var packFromOption = new Option<Guid?>("--pack-from")
        {
            Description = "GUID of the source article this pack is composed of."
        };

        var packQuantityOption = new Option<double?>("--pack-quantity")
        {
            Description = "Quantity of the source article contained in the pack."
        };

        var containerMaxMassOption = new Option<string?>("--container-max-mass")
        {
            Description = "Maximum mass the container can hold, as value+unit, e.g. '25kg'."
        };

        var barcodeEan13Option = new Option<string?>("--barcode-ean13")
        {
            Description = "Optional EAN-13 barcode to assign to the article."
        };

        var kitComponentOption = new Option<string[]?>("--kit-component")
        {
            Description =
                "Kit component as '<article-guid>' or '<article-guid>:<quantity>'. "
                + "Repeat the option for multiple components.",
            AllowMultipleArgumentsPerToken = true
        };

        var partitionsOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Partition GUIDs to associate the article with (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(nameOption);
        Add(typeOption);
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
        Add(containerMaxMassOption);
        Add(barcodeEan13Option);
        Add(kitComponentOption);
        Add(partitionsOption);

        SetAction(async parseResult =>
        {
            var name = parseResult.GetValue(nameOption)!;
            var type = parseResult.GetValue(typeOption);
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
            var containerMaxMass = parseResult.GetValue(containerMaxMassOption);
            var barcodeEan13 = parseResult.GetValue(barcodeEan13Option);
            var kitComponents = parseResult.GetValue(kitComponentOption);
            var partitions = parseResult.GetValue(partitionsOption);

            var dto = new ArticleCreateDto
            {
                Name = name,
                ArticleType = (int)type,
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

            var guid = await client.Articles.PostAsync(dto);

            StatusWriter.Success(
                "Article created.",
                ("GUID", guid?.ToString()),
                ("Name", name));

            return 0;
        });
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
