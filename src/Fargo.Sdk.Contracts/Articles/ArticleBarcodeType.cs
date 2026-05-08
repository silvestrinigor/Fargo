namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Supported article barcode symbologies.</summary>
public enum ArticleBarcodeType
{
    Ean13 = 0,
    Ean8 = 1,
    UpcA = 2,
    UpcE = 3,
    Code128 = 4,
    Code39 = 5,
    Itf14 = 6,
    Gs1128 = 7,
    QrCode = 8,
    DataMatrix = 9,
}
