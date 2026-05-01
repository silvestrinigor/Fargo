namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents one barcode value in API contracts.</summary>
public sealed record BarcodeValueDto(string Code);

/// <summary>Represents article barcodes grouped by symbology in API contracts.</summary>
public sealed record ArticleBarcodesDto(
    BarcodeValueDto? Ean13 = null,
    BarcodeValueDto? Ean8 = null,
    BarcodeValueDto? UpcA = null,
    BarcodeValueDto? UpcE = null,
    BarcodeValueDto? Code128 = null,
    BarcodeValueDto? Code39 = null,
    BarcodeValueDto? Itf14 = null,
    BarcodeValueDto? Gs1128 = null,
    BarcodeValueDto? QrCode = null,
    BarcodeValueDto? DataMatrix = null);

