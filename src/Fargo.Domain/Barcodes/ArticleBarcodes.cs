using Fargo.Domain.Articles;

namespace Fargo.Domain.Barcodes;

/// <summary>
/// Domain facade for an article's barcode collection, grouped by barcode format.
/// Properties are <see langword="null"/> when the article has no barcode in that format.
/// </summary>
public sealed class ArticleBarcodes
{
    /// <summary>Initializes an empty detached barcode group.</summary>
    public ArticleBarcodes()
    {
        barcodes = [];
    }

    internal ArticleBarcodes(Article article, BarcodeCollection barcodes)
    {
        this.article = article;
        this.barcodes = barcodes;
    }

    private readonly Article? article;
    private readonly BarcodeCollection barcodes;

    /// <summary>EAN-13 barcode, or <see langword="null"/> when absent.</summary>
    public Ean13? Ean13
    {
        get => Get(BarcodeFormat.Ean13, b => new Ean13(b.Guid, b.ArticleGuid, b.Code));
        set => Set(value?.Code, BarcodeFormat.Ean13);
    }

    /// <summary>EAN-8 barcode, or <see langword="null"/> when absent.</summary>
    public Ean8? Ean8
    {
        get => Get(BarcodeFormat.Ean8, b => new Ean8(b.Guid, b.ArticleGuid, b.Code));
        set => Set(value?.Code, BarcodeFormat.Ean8);
    }

    /// <summary>UPC-A barcode, or <see langword="null"/> when absent.</summary>
    public UpcA? UpcA
    {
        get => Get(BarcodeFormat.UpcA, b => new UpcA(b.Guid, b.ArticleGuid, b.Code));
        set => Set(value?.Code, BarcodeFormat.UpcA);
    }

    /// <summary>UPC-E barcode, or <see langword="null"/> when absent.</summary>
    public UpcE? UpcE
    {
        get => Get(BarcodeFormat.UpcE, b => new UpcE(b.Guid, b.ArticleGuid, b.Code));
        set => Set(value?.Code, BarcodeFormat.UpcE);
    }

    /// <summary>Code 128 barcode, or <see langword="null"/> when absent.</summary>
    public Code128? Code128
    {
        get => Get(BarcodeFormat.Code128, b => new Code128(b.Guid, b.ArticleGuid, b.Code));
        set => Set(value?.Code, BarcodeFormat.Code128);
    }

    /// <summary>Code 39 barcode, or <see langword="null"/> when absent.</summary>
    public Code39? Code39
    {
        get => Get(BarcodeFormat.Code39, b => new Code39(b.Guid, b.ArticleGuid, b.Code));
        set => Set(value?.Code, BarcodeFormat.Code39);
    }

    /// <summary>ITF-14 barcode, or <see langword="null"/> when absent.</summary>
    public Itf14? Itf14
    {
        get => Get(BarcodeFormat.Itf14, b => new Itf14(b.Guid, b.ArticleGuid, b.Code));
        set => Set(value?.Code, BarcodeFormat.Itf14);
    }

    /// <summary>GS1-128 barcode, or <see langword="null"/> when absent.</summary>
    public Gs1128? Gs1128
    {
        get => Get(BarcodeFormat.Gs1128, b => new Gs1128(b.Guid, b.ArticleGuid, b.Code));
        set => Set(value?.Code, BarcodeFormat.Gs1128);
    }

    /// <summary>QR Code barcode, or <see langword="null"/> when absent.</summary>
    public QrCode? QrCode
    {
        get => Get(BarcodeFormat.QrCode, b => new QrCode(b.Guid, b.ArticleGuid, b.Code));
        set => Set(value?.Code, BarcodeFormat.QrCode);
    }

    /// <summary>Data Matrix barcode, or <see langword="null"/> when absent.</summary>
    public DataMatrix? DataMatrix
    {
        get => Get(BarcodeFormat.DataMatrix, b => new DataMatrix(b.Guid, b.ArticleGuid, b.Code));
        set => Set(value?.Code, BarcodeFormat.DataMatrix);
    }

    /// <summary>Gets whether this article has no barcodes in any supported format.</summary>
    public bool IsEmpty => barcodes.Count == 0;

    /// <summary>Creates a detached typed barcode group from persisted barcode entities.</summary>
    public static ArticleBarcodes From(IEnumerable<Barcode> barcodes)
    {
        ArgumentNullException.ThrowIfNull(barcodes);

        var result = new ArticleBarcodes();
        foreach (var barcode in barcodes)
        {
            result.barcodes.Add(barcode);
        }

        return result;
    }

    /// <summary>Returns whether a barcode exists for the specified format.</summary>
    public bool Contains(BarcodeFormat format) => barcodes.Any(b => b.Format == format);

    /// <summary>Adds or replaces the barcode for the value's format.</summary>
    public Barcode Set(BarcodeValue value)
    {
        var current = barcodes.SingleOrDefault(b => b.Format == value.Format);
        if (current is not null)
        {
            barcodes.Remove(current);
        }

        return Add(value);
    }

    /// <summary>Removes a barcode by identifier.</summary>
    public bool Remove(Guid barcodeGuid)
    {
        var current = barcodes.SingleOrDefault(b => b.Guid == barcodeGuid);
        if (current is null)
        {
            return false;
        }

        barcodes.Remove(current);
        return true;
    }

    private T? Get<T>(BarcodeFormat format, Func<Barcode, T> map) where T : struct
    {
        var barcode = barcodes.SingleOrDefault(b => b.Format == format);
        return barcode is null ? null : map(barcode);
    }

    private void Set(string? code, BarcodeFormat format)
    {
        var current = barcodes.SingleOrDefault(b => b.Format == format);
        if (current is not null)
        {
            barcodes.Remove(current);
        }

        if (code is not null)
        {
            Add(new BarcodeValue(code, format));
        }
    }

    private Barcode Add(BarcodeValue value)
    {
        if (article is null)
        {
            throw new InvalidOperationException("Detached article barcodes cannot be mutated.");
        }

        var barcode = new Barcode
        {
            Value = value,
            Article = article
        };

        barcodes.Add(barcode);
        return barcode;
    }
}

/// <summary>EAN-13 barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct Ean13(Guid Guid, Guid ArticleGuid, string Code)
{
    /// <summary>Initializes a new detached EAN-13 value.</summary>
    public Ean13(string code) : this(Guid.Empty, Guid.Empty, code) { }
}

/// <summary>EAN-8 barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct Ean8(Guid Guid, Guid ArticleGuid, string Code)
{
    /// <summary>Initializes a new detached EAN-8 value.</summary>
    public Ean8(string code) : this(Guid.Empty, Guid.Empty, code) { }
}

/// <summary>UPC-A barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct UpcA(Guid Guid, Guid ArticleGuid, string Code)
{
    /// <summary>Initializes a new detached UPC-A value.</summary>
    public UpcA(string code) : this(Guid.Empty, Guid.Empty, code) { }
}

/// <summary>UPC-E barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct UpcE(Guid Guid, Guid ArticleGuid, string Code)
{
    /// <summary>Initializes a new detached UPC-E value.</summary>
    public UpcE(string code) : this(Guid.Empty, Guid.Empty, code) { }
}

/// <summary>Code 128 barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct Code128(Guid Guid, Guid ArticleGuid, string Code)
{
    /// <summary>Initializes a new detached Code 128 value.</summary>
    public Code128(string code) : this(Guid.Empty, Guid.Empty, code) { }
}

/// <summary>Code 39 barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct Code39(Guid Guid, Guid ArticleGuid, string Code)
{
    /// <summary>Initializes a new detached Code 39 value.</summary>
    public Code39(string code) : this(Guid.Empty, Guid.Empty, code) { }
}

/// <summary>ITF-14 barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct Itf14(Guid Guid, Guid ArticleGuid, string Code)
{
    /// <summary>Initializes a new detached ITF-14 value.</summary>
    public Itf14(string code) : this(Guid.Empty, Guid.Empty, code) { }
}

/// <summary>GS1-128 barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct Gs1128(Guid Guid, Guid ArticleGuid, string Code)
{
    /// <summary>Initializes a new detached GS1-128 value.</summary>
    public Gs1128(string code) : this(Guid.Empty, Guid.Empty, code) { }
}

/// <summary>QR Code barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct QrCode(Guid Guid, Guid ArticleGuid, string Code)
{
    /// <summary>Initializes a new detached QR Code value.</summary>
    public QrCode(string code) : this(Guid.Empty, Guid.Empty, code) { }
}

/// <summary>Data Matrix barcode value assigned to an article.</summary>
/// <param name="Guid">The unique barcode identifier.</param>
/// <param name="ArticleGuid">The owning article identifier.</param>
/// <param name="Code">The barcode code.</param>
public readonly record struct DataMatrix(Guid Guid, Guid ArticleGuid, string Code)
{
    /// <summary>Initializes a new detached Data Matrix value.</summary>
    public DataMatrix(string code) : this(Guid.Empty, Guid.Empty, code) { }
}
