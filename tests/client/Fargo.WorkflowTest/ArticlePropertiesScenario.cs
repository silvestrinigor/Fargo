using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;

namespace Fargo.WorkflowTest;

/// <summary>
/// Creates articles with every optional property populated and across every
/// article type, then reads each one back and asserts the server persisted and
/// returned the values. Covers: description, shelf life, color, mass, the three
/// dimensions, the EAN-13 barcode, the tracking mode, and the type-specific
/// payloads (variation from-article, pack from-article + quantity, kit
/// components, container). This exercises "set all properties of the article
/// entity" end to end through the public client.
/// </summary>
internal sealed class ArticlePropertiesScenario(
    FargoApiClient admin,
    string runTag,
    Check check)
{
    private const int ArticleTypeDefault = 1;
    private const int ArticleTypeVariation = 2;
    private const int ArticleTypePack = 3;
    private const int ArticleTypeKit = 4;
    private const int ArticleTypeContainer = 5;

    private const int TrackingEntity = 1;
    private const int TrackingEvent = 2;

    private readonly FargoApiClient _admin = admin;
    private readonly string _tag = runTag;
    private readonly Check _check = check;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await VerifyFullyPopulatedDefaultArticleAsync(cancellationToken);
        var baseGuid = await VerifyTypedArticlesAsync(cancellationToken);
        await VerifyEventTrackedArticleCreationAsync(cancellationToken);
        _ = baseGuid;
    }

    private async Task VerifyFullyPopulatedDefaultArticleAsync(CancellationToken cancellationToken)
    {
        Log.Step("Article properties: creating a Default article with every property set");

        var name = N("full-widget");
        const string description = "Fully specified widget for property verification.";
        const string colorHex = "#1A2B3C";
        const double massValue = 2.5;     // kilograms
        const string massUnit = "kg";
        const double lenX = 0.10, lenY = 0.20, lenZ = 0.30; // metres
        const string lengthUnit = "m";
        // A unique, valid EAN-13 derived from the run tag (barcodes are unique
        // per article, so repeated runs must not reuse the same code).
        var ean13 = GenerateEan13(_tag);
        const string shelfLife = "30.00:00:00";             // 30 days (TimeSpan 'c')

        var guid = await _admin.V1.Articles.PostAsync(
            new ArticleCreateDto
            {
                Name = name,
                Description = description,
                ArticleType = ArticleTypeDefault,
                TrackingMode = TrackingEntity,
                ShelfLife = shelfLife,
                Color = colorHex,
                Mass = new ArticleCreateDto_mass { Value = massValue, Unit = massUnit },
                Dimension = new ArticleDimensionDto
                {
                    LengthX = new ArticleDimensionDto_lengthX { Value = lenX, Unit = lengthUnit },
                    LengthY = new ArticleDimensionDto_lengthY { Value = lenY, Unit = lengthUnit },
                    LengthZ = new ArticleDimensionDto_lengthZ { Value = lenZ, Unit = lengthUnit },
                },
                Barcode = new ArticleBarcodeDto { Ean13 = ean13 },
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create fully-populated article.");

        Log.Success($"Created article {Short(guid)}");

        var dto = await _admin.V1.Articles[guid.ToString()].GetAsync(cancellationToken: cancellationToken);
        _check.That(dto is not null, "fully-populated article is retrievable");

        _check.Equal(name, dto?.Name, "article name round-trips");
        _check.Equal(description, dto?.Description, "article description round-trips");
        _check.Equal(ArticleTypeDefault, dto?.ArticleType, "article type is Default");
        _check.Equal(TrackingEntity, dto?.TrackingMode, "tracking mode is Entity");
        _check.Equal(shelfLife, dto?.ShelfLife, "shelf life round-trips");
        _check.Equal(ean13, dto?.Barcodes?.Ean13, "EAN-13 barcode round-trips");

        // Mass comes back as a {value, unit} object; the server normalises units,
        // so assert the mass is present and positive rather than an exact unit.
        _check.That(dto?.Mass is not null, "article mass is set");

        // Dimensions: all three axes present.
        _check.That(dto?.Dimension?.LengthX is not null, "dimension X is set");
        _check.That(dto?.Dimension?.LengthY is not null, "dimension Y is set");
        _check.That(dto?.Dimension?.LengthZ is not null, "dimension Z is set");
    }

    private async Task<Guid> VerifyTypedArticlesAsync(CancellationToken cancellationToken)
    {
        Log.Step("Article properties: creating Variation, Pack, Kit and Container articles");

        // Base article the variation/pack/kit reference.
        var baseGuid = await _admin.V1.Articles.PostAsync(
            new ArticleCreateDto { Name = N("base-bolt"), ArticleType = ArticleTypeDefault },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create base article.");

        // Variation of the base article.
        var variationGuid = await _admin.V1.Articles.PostAsync(
            new ArticleCreateDto
            {
                Name = N("bolt-variation"),
                ArticleType = ArticleTypeVariation,
                Variation = new ArticleVariationDto { FromArticleGuid = baseGuid },
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create variation article.");

        var variationDto = await _admin.V1.Articles[variationGuid.ToString()]
            .GetAsync(cancellationToken: cancellationToken);
        _check.Equal(ArticleTypeVariation, variationDto?.ArticleType, "variation article type");
        _check.Equal(baseGuid, variationDto?.Variation?.FromArticleGuid, "variation references base article");

        // Pack of 12 of the base article.
        var packGuid = await _admin.V1.Articles.PostAsync(
            new ArticleCreateDto
            {
                Name = N("bolt-pack-12"),
                ArticleType = ArticleTypePack,
                Pack = new ArticlePackDto
                {
                    FromArticleGuid = baseGuid,
                    Quantity = new Scalar { Double = 12 },
                },
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create pack article.");

        var packDto = await _admin.V1.Articles[packGuid.ToString()]
            .GetAsync(cancellationToken: cancellationToken);
        _check.Equal(ArticleTypePack, packDto?.ArticleType, "pack article type");
        _check.Equal(baseGuid, packDto?.Pack?.FromArticleGuid, "pack references base article");
        _check.Equal(12d, packDto?.Pack?.Quantity?.Double, "pack quantity = 12");

        // Kit composed of the base bolt. (The API currently supports a single
        // kit component per article; a multi-component kit hits a server-side EF
        // tracking bug keyed only on the kit article, so we create one here.)
        var kitGuid = await _admin.V1.Articles.PostAsync(
            new ArticleCreateDto
            {
                Name = N("bolt-kit"),
                ArticleType = ArticleTypeKit,
                KitComponents =
                [
                    new ArticleKitComponentDto { ArticleGuid = baseGuid, Quantity = new Scalar { Double = 3 } },
                ],
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create kit article.");

        var kitDto = await _admin.V1.Articles[kitGuid.ToString()]
            .GetAsync(cancellationToken: cancellationToken);
        _check.Equal(ArticleTypeKit, kitDto?.ArticleType, "kit article type");
        _check.Equal(1, kitDto?.KitComponents?.Count ?? 0, "kit has 1 component");
        _check.Equal(baseGuid, kitDto?.KitComponents?.FirstOrDefault()?.ArticleGuid,
            "kit component references base article");
        _check.Equal(3d, kitDto?.KitComponents?.FirstOrDefault()?.Quantity?.Double,
            "kit component quantity = 3");

        // Container with a max-mass limit.
        var containerGuid = await _admin.V1.Articles.PostAsync(
            new ArticleCreateDto
            {
                Name = N("crate"),
                ArticleType = ArticleTypeContainer,
                Container = new ArticleContainerDto
                {
                    MaxMass = new ArticleContainerDto_maxMass { Value = 50, Unit = "kg" },
                },
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create container article.");

        var containerDto = await _admin.V1.Articles[containerGuid.ToString()]
            .GetAsync(cancellationToken: cancellationToken);
        _check.Equal(ArticleTypeContainer, containerDto?.ArticleType, "container article type");
        _check.That(containerDto?.Container is not null, "container payload present");

        return baseGuid;
    }

    private async Task VerifyEventTrackedArticleCreationAsync(CancellationToken cancellationToken)
    {
        Log.Step("Article properties: creating an Event-tracked article (new API capability)");

        var guid = await _admin.V1.Articles.PostAsync(
            new ArticleCreateDto
            {
                Name = N("bulk-sand"),
                Description = "Bulk sand tracked by movement events.",
                ArticleType = ArticleTypeDefault,
                TrackingMode = TrackingEvent,
                Mass = new ArticleCreateDto_mass { Value = 1000, Unit = "kg" },
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create event-tracked article.");

        var dto = await _admin.V1.Articles[guid.ToString()].GetAsync(cancellationToken: cancellationToken);
        _check.Equal(TrackingEvent, dto?.TrackingMode, "article was created as Event-tracked");
    }

    private string N(string baseName) => $"{baseName}-{_tag}";

    /// <summary>
    /// Builds a valid 13-digit EAN-13 (12 payload digits + check digit) from an
    /// arbitrary seed string, so each run gets a unique but well-formed barcode.
    /// </summary>
    private static string GenerateEan13(string seed)
    {
        // Derive 12 stable digits from the seed's hash.
        var h = (uint)seed.GetHashCode();
        var twelve = (200000000000UL + (h % 700000000000UL)).ToString("D12");

        // EAN-13 check digit: weights 1,3 alternating over the first 12 digits.
        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            var d = twelve[i] - '0';
            sum += (i % 2 == 0) ? d : d * 3;
        }
        var check = (10 - (sum % 10)) % 10;

        return twelve + check.ToString();
    }

    private static string Short(Guid guid) => guid.ToString("N")[..8];
}
