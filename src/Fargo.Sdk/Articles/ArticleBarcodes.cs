namespace Fargo.Sdk.Articles;

/// <summary>
/// Groups an article's barcodes by symbology. A property is <see langword="null"/>
/// when the article has no barcode in that format.
/// </summary>
public sealed record ArticleBarcodes
{
    /// <summary>Initializes an empty barcode group.</summary>
    public ArticleBarcodes()
    {
    }

    /// <summary>EAN-13 barcode, or <see langword="null"/> when absent.</summary>
    public Ean13? Ean13 { get; init; }

    /// <summary>EAN-8 barcode, or <see langword="null"/> when absent.</summary>
    public Ean8? Ean8 { get; init; }

    /// <summary>UPC-A barcode, or <see langword="null"/> when absent.</summary>
    public UpcA? UpcA { get; init; }

    /// <summary>UPC-E barcode, or <see langword="null"/> when absent.</summary>
    public UpcE? UpcE { get; init; }

    /// <summary>Code 128 barcode, or <see langword="null"/> when absent.</summary>
    public Code128? Code128 { get; init; }

    /// <summary>Code 39 barcode, or <see langword="null"/> when absent.</summary>
    public Code39? Code39 { get; init; }

    /// <summary>ITF-14 barcode, or <see langword="null"/> when absent.</summary>
    public Itf14? Itf14 { get; init; }

    /// <summary>GS1-128 barcode, or <see langword="null"/> when absent.</summary>
    public Gs1128? Gs1128 { get; init; }

    /// <summary>QR Code barcode, or <see langword="null"/> when absent.</summary>
    public QrCode? QrCode { get; init; }

    /// <summary>Data Matrix barcode, or <see langword="null"/> when absent.</summary>
    public DataMatrix? DataMatrix { get; init; }

    /// <summary>Gets whether this article has no barcodes in any supported format.</summary>
    public bool IsEmpty =>
        Ean13 is null &&
        Ean8 is null &&
        UpcA is null &&
        UpcE is null &&
        Code128 is null &&
        Code39 is null &&
        Itf14 is null &&
        Gs1128 is null &&
        QrCode is null &&
        DataMatrix is null;

    /// <summary>Creates a typed barcode group from legacy barcode result rows.</summary>
    public static ArticleBarcodes From(IEnumerable<BarcodeResult> barcodes)
    {
        ArgumentNullException.ThrowIfNull(barcodes);

        var result = new ArticleBarcodes();

        foreach (var barcode in barcodes)
        {
            result = barcode.Format switch
            {
                BarcodeFormat.Ean13 => result with { Ean13 = new Ean13(barcode.Guid, barcode.ArticleGuid, barcode.Code) },
                BarcodeFormat.Ean8 => result with { Ean8 = new Ean8(barcode.Guid, barcode.ArticleGuid, barcode.Code) },
                BarcodeFormat.UpcA => result with { UpcA = new UpcA(barcode.Guid, barcode.ArticleGuid, barcode.Code) },
                BarcodeFormat.UpcE => result with { UpcE = new UpcE(barcode.Guid, barcode.ArticleGuid, barcode.Code) },
                BarcodeFormat.Code128 => result with { Code128 = new Code128(barcode.Guid, barcode.ArticleGuid, barcode.Code) },
                BarcodeFormat.Code39 => result with { Code39 = new Code39(barcode.Guid, barcode.ArticleGuid, barcode.Code) },
                BarcodeFormat.Itf14 => result with { Itf14 = new Itf14(barcode.Guid, barcode.ArticleGuid, barcode.Code) },
                BarcodeFormat.Gs1128 => result with { Gs1128 = new Gs1128(barcode.Guid, barcode.ArticleGuid, barcode.Code) },
                BarcodeFormat.QrCode => result with { QrCode = new QrCode(barcode.Guid, barcode.ArticleGuid, barcode.Code) },
                BarcodeFormat.DataMatrix => result with { DataMatrix = new DataMatrix(barcode.Guid, barcode.ArticleGuid, barcode.Code) },
                _ => result
            };
        }

        return result;
    }

    /// <summary>Enumerates the populated barcode slots as general barcode results.</summary>
    public IEnumerable<BarcodeResult> AsResults()
    {
        if (Ean13 is { } ean13)
        {
            yield return new BarcodeResult(ean13.Guid, ean13.ArticleGuid, ean13.Code, BarcodeFormat.Ean13);
        }

        if (Ean8 is { } ean8)
        {
            yield return new BarcodeResult(ean8.Guid, ean8.ArticleGuid, ean8.Code, BarcodeFormat.Ean8);
        }

        if (UpcA is { } upcA)
        {
            yield return new BarcodeResult(upcA.Guid, upcA.ArticleGuid, upcA.Code, BarcodeFormat.UpcA);
        }

        if (UpcE is { } upcE)
        {
            yield return new BarcodeResult(upcE.Guid, upcE.ArticleGuid, upcE.Code, BarcodeFormat.UpcE);
        }

        if (Code128 is { } code128)
        {
            yield return new BarcodeResult(code128.Guid, code128.ArticleGuid, code128.Code, BarcodeFormat.Code128);
        }

        if (Code39 is { } code39)
        {
            yield return new BarcodeResult(code39.Guid, code39.ArticleGuid, code39.Code, BarcodeFormat.Code39);
        }

        if (Itf14 is { } itf14)
        {
            yield return new BarcodeResult(itf14.Guid, itf14.ArticleGuid, itf14.Code, BarcodeFormat.Itf14);
        }

        if (Gs1128 is { } gs1128)
        {
            yield return new BarcodeResult(gs1128.Guid, gs1128.ArticleGuid, gs1128.Code, BarcodeFormat.Gs1128);
        }

        if (QrCode is { } qrCode)
        {
            yield return new BarcodeResult(qrCode.Guid, qrCode.ArticleGuid, qrCode.Code, BarcodeFormat.QrCode);
        }

        if (DataMatrix is { } dataMatrix)
        {
            yield return new BarcodeResult(dataMatrix.Guid, dataMatrix.ArticleGuid, dataMatrix.Code, BarcodeFormat.DataMatrix);
        }
    }
}

/// <summary>EAN-13 barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct Ean13(Guid Guid, Guid ArticleGuid, string Code);

/// <summary>EAN-8 barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct Ean8(Guid Guid, Guid ArticleGuid, string Code);

/// <summary>UPC-A barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct UpcA(Guid Guid, Guid ArticleGuid, string Code);

/// <summary>UPC-E barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct UpcE(Guid Guid, Guid ArticleGuid, string Code);

/// <summary>Code 128 barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct Code128(Guid Guid, Guid ArticleGuid, string Code);

/// <summary>Code 39 barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct Code39(Guid Guid, Guid ArticleGuid, string Code);

/// <summary>ITF-14 barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct Itf14(Guid Guid, Guid ArticleGuid, string Code);

/// <summary>GS1-128 barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct Gs1128(Guid Guid, Guid ArticleGuid, string Code);

/// <summary>QR Code barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct QrCode(Guid Guid, Guid ArticleGuid, string Code);

/// <summary>Data Matrix barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct DataMatrix(Guid Guid, Guid ArticleGuid, string Code);
