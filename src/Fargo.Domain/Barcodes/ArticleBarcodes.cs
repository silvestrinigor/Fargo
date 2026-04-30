using System.ComponentModel.DataAnnotations.Schema;

namespace Fargo.Domain.Barcodes;

// EF Core owned entity backing types — one per barcode format.
// These are public so that EF Core infrastructure can reference them in generic type parameters.
// ArticleGuid is the PK+FK in each dedicated barcode table.

/// <summary>EF Core backing data for an EAN-13 barcode row.</summary>
public sealed class Ean13Data { public Guid ArticleGuid { get; set; } public required string Code { get; set; } }

/// <summary>EF Core backing data for an EAN-8 barcode row.</summary>
public sealed class Ean8Data { public Guid ArticleGuid { get; set; } public required string Code { get; set; } }

/// <summary>EF Core backing data for a UPC-A barcode row.</summary>
public sealed class UpcAData { public Guid ArticleGuid { get; set; } public required string Code { get; set; } }

/// <summary>EF Core backing data for a UPC-E barcode row.</summary>
public sealed class UpcEData { public Guid ArticleGuid { get; set; } public required string Code { get; set; } }

/// <summary>EF Core backing data for a Code 128 barcode row.</summary>
public sealed class Code128Data { public Guid ArticleGuid { get; set; } public required string Code { get; set; } }

/// <summary>EF Core backing data for a Code 39 barcode row.</summary>
public sealed class Code39Data { public Guid ArticleGuid { get; set; } public required string Code { get; set; } }

/// <summary>EF Core backing data for an ITF-14 barcode row.</summary>
public sealed class Itf14Data { public Guid ArticleGuid { get; set; } public required string Code { get; set; } }

/// <summary>EF Core backing data for a GS1-128 barcode row.</summary>
public sealed class Gs1128Data { public Guid ArticleGuid { get; set; } public required string Code { get; set; } }

/// <summary>EF Core backing data for a QR Code barcode row.</summary>
public sealed class QrCodeData { public Guid ArticleGuid { get; set; } public required string Code { get; set; } }

/// <summary>EF Core backing data for a Data Matrix barcode row.</summary>
public sealed class DataMatrixData { public Guid ArticleGuid { get; set; } public required string Code { get; set; } }

/// <summary>
/// Domain facade for an article's barcode collection, grouped by barcode format.
/// Properties are <see langword="null"/> when the article has no barcode in that format.
/// </summary>
public sealed class ArticleBarcodes
{
    /// <summary>Initializes an empty detached barcode group.</summary>
    public ArticleBarcodes()
    {
    }

    internal Ean13Data? Ean13Data { get; set; }
    internal Ean8Data? Ean8Data { get; set; }
    internal UpcAData? UpcAData { get; set; }
    internal UpcEData? UpcEData { get; set; }
    internal Code128Data? Code128Data { get; set; }
    internal Code39Data? Code39Data { get; set; }
    internal Itf14Data? Itf14Data { get; set; }
    internal Gs1128Data? Gs1128Data { get; set; }
    internal QrCodeData? QrCodeData { get; set; }
    internal DataMatrixData? DataMatrixData { get; set; }

    internal void ReplaceWith(ArticleBarcodes source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Ean13Data = source.Ean13Data is null ? null : new Ean13Data { Code = source.Ean13Data.Code };
        Ean8Data = source.Ean8Data is null ? null : new Ean8Data { Code = source.Ean8Data.Code };
        UpcAData = source.UpcAData is null ? null : new UpcAData { Code = source.UpcAData.Code };
        UpcEData = source.UpcEData is null ? null : new UpcEData { Code = source.UpcEData.Code };
        Code128Data = source.Code128Data is null ? null : new Code128Data { Code = source.Code128Data.Code };
        Code39Data = source.Code39Data is null ? null : new Code39Data { Code = source.Code39Data.Code };
        Itf14Data = source.Itf14Data is null ? null : new Itf14Data { Code = source.Itf14Data.Code };
        Gs1128Data = source.Gs1128Data is null ? null : new Gs1128Data { Code = source.Gs1128Data.Code };
        QrCodeData = source.QrCodeData is null ? null : new QrCodeData { Code = source.QrCodeData.Code };
        DataMatrixData = source.DataMatrixData is null ? null : new DataMatrixData { Code = source.DataMatrixData.Code };
    }

    /// <summary>EAN-13 barcode, or <see langword="null"/> when absent.</summary>
    [NotMapped]
    public Ean13? Ean13
    {
        get => Ean13Data is null ? null : new Ean13(Ean13Data.Code);
        set => Ean13Data = value is null ? null : new Ean13Data { Code = value.Value.Code };
    }

    /// <summary>EAN-8 barcode, or <see langword="null"/> when absent.</summary>
    [NotMapped]
    public Ean8? Ean8
    {
        get => Ean8Data is null ? null : new Ean8(Ean8Data.Code);
        set => Ean8Data = value is null ? null : new Ean8Data { Code = value.Value.Code };
    }

    /// <summary>UPC-A barcode, or <see langword="null"/> when absent.</summary>
    [NotMapped]
    public UpcA? UpcA
    {
        get => UpcAData is null ? null : new UpcA(UpcAData.Code);
        set => UpcAData = value is null ? null : new UpcAData { Code = value.Value.Code };
    }

    /// <summary>UPC-E barcode, or <see langword="null"/> when absent.</summary>
    [NotMapped]
    public UpcE? UpcE
    {
        get => UpcEData is null ? null : new UpcE(UpcEData.Code);
        set => UpcEData = value is null ? null : new UpcEData { Code = value.Value.Code };
    }

    /// <summary>Code 128 barcode, or <see langword="null"/> when absent.</summary>
    [NotMapped]
    public Code128? Code128
    {
        get => Code128Data is null ? null : new Code128(Code128Data.Code);
        set => Code128Data = value is null ? null : new Code128Data { Code = value.Value.Code };
    }

    /// <summary>Code 39 barcode, or <see langword="null"/> when absent.</summary>
    [NotMapped]
    public Code39? Code39
    {
        get => Code39Data is null ? null : new Code39(Code39Data.Code);
        set => Code39Data = value is null ? null : new Code39Data { Code = value.Value.Code };
    }

    /// <summary>ITF-14 barcode, or <see langword="null"/> when absent.</summary>
    [NotMapped]
    public Itf14? Itf14
    {
        get => Itf14Data is null ? null : new Itf14(Itf14Data.Code);
        set => Itf14Data = value is null ? null : new Itf14Data { Code = value.Value.Code };
    }

    /// <summary>GS1-128 barcode, or <see langword="null"/> when absent.</summary>
    [NotMapped]
    public Gs1128? Gs1128
    {
        get => Gs1128Data is null ? null : new Gs1128(Gs1128Data.Code);
        set => Gs1128Data = value is null ? null : new Gs1128Data { Code = value.Value.Code };
    }

    /// <summary>QR Code barcode, or <see langword="null"/> when absent.</summary>
    [NotMapped]
    public QrCode? QrCode
    {
        get => QrCodeData is null ? null : new QrCode(QrCodeData.Code);
        set => QrCodeData = value is null ? null : new QrCodeData { Code = value.Value.Code };
    }

    /// <summary>Data Matrix barcode, or <see langword="null"/> when absent.</summary>
    [NotMapped]
    public DataMatrix? DataMatrix
    {
        get => DataMatrixData is null ? null : new DataMatrix(DataMatrixData.Code);
        set => DataMatrixData = value is null ? null : new DataMatrixData { Code = value.Value.Code };
    }

    /// <summary>Gets whether this article has no barcodes in any supported format.</summary>
    [NotMapped]
    public bool IsEmpty =>
        Ean13Data is null && Ean8Data is null && UpcAData is null && UpcEData is null &&
        Code128Data is null && Code39Data is null && Itf14Data is null && Gs1128Data is null &&
        QrCodeData is null && DataMatrixData is null;

    /// <summary>Enumerates the populated barcode values.</summary>
    public IEnumerable<BarcodeValue> AsValues()
    {
        if (Ean13Data is not null) yield return BarcodeValue.Ean13(Ean13Data.Code);
        if (Ean8Data is not null) yield return BarcodeValue.Ean8(Ean8Data.Code);
        if (UpcAData is not null) yield return BarcodeValue.UpcA(UpcAData.Code);
        if (UpcEData is not null) yield return BarcodeValue.UpcE(UpcEData.Code);
        if (Code128Data is not null) yield return BarcodeValue.Code128(Code128Data.Code);
        if (Code39Data is not null) yield return BarcodeValue.Code39(Code39Data.Code);
        if (Itf14Data is not null) yield return BarcodeValue.Itf14(Itf14Data.Code);
        if (Gs1128Data is not null) yield return BarcodeValue.Gs1128(Gs1128Data.Code);
        if (QrCodeData is not null) yield return BarcodeValue.QrCode(QrCodeData.Code);
        if (DataMatrixData is not null) yield return BarcodeValue.DataMatrix(DataMatrixData.Code);
    }
}

/// <summary>EAN-13 barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct Ean13(string Code);

/// <summary>EAN-8 barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct Ean8(string Code);

/// <summary>UPC-A barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct UpcA(string Code);

/// <summary>UPC-E barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct UpcE(string Code);

/// <summary>Code 128 barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct Code128(string Code);

/// <summary>Code 39 barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct Code39(string Code);

/// <summary>ITF-14 barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct Itf14(string Code);

/// <summary>GS1-128 barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct Gs1128(string Code);

/// <summary>QR Code barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct QrCode(string Code);

/// <summary>Data Matrix barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct DataMatrix(string Code);
