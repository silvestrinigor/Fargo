namespace Fargo.Api.Contracts.Articles;

/// <summary>Represents article barcodes grouped by symbology in API contracts.</summary>
public sealed record ArticleBarcodesDto(
    string? Ean13 = null,
    string? Ean8 = null,
    string? UpcA = null,
    string? UpcE = null,
    string? Code128 = null,
    string? Code39 = null,
    string? Itf14 = null,
    string? Gs1128 = null,
    string? QrCode = null,
    string? DataMatrix = null);
