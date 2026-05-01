using Fargo.Domain.Barcodes;

namespace Fargo.Domain.Articles;

/// <summary>
/// Domain facade for an article's barcode collection, grouped by barcode format.
/// Properties are <see langword="null"/> when the article has no barcode in that format.
/// </summary>
public sealed class ArticleBarcodes
{
    /// <summary>
    /// Initializes an empty detached barcode group.
    /// </summary>
    public ArticleBarcodes()
    {
    }

    /// <summary>
    /// EAN-13 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Ean13? Ean13 { get; set; }

    /// <summary>
    /// EAN-8 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Ean8? Ean8 { get; set; }

    /// <summary>
    /// UPC-A barcode, or <see langword="null"/> when absent.
    /// </summary>
    public UpcA? UpcA { get; set; }

    /// <summary>
    /// UPC-E barcode, or <see langword="null"/> when absent.
    /// </summary>
    public UpcE? UpcE { get; set; }

    /// <summary>
    /// Code 128 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Code128? Code128 { get; set; }

    /// <summary>
    /// Code 39 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Code39? Code39 { get; set; }

    /// <summary>
    /// ITF-14 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Itf14? Itf14 { get; set; }

    /// <summary>
    /// GS1-128 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Gs1128? Gs1128 { get; set; }

    /// <summary>
    /// QR Code barcode, or <see langword="null"/> when absent.
    /// </summary>
    public QrCode? QrCode { get; set; }

    /// <summary>
    /// Data Matrix barcode, or <see langword="null"/> when absent.
    /// </summary>
    public DataMatrix? DataMatrix { get; set; }

    /// <summary>
    /// Gets whether this article has no barcodes in any supported format.
    /// </summary>
    public bool IsEmpty =>
        Ean13 is null && Ean8 is null && UpcA is null && UpcE is null &&
        Code128 is null && Code39 is null && Itf14 is null && Gs1128 is null &&
        QrCode is null && DataMatrix is null;
}
