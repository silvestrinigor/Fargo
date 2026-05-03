namespace Fargo.Application.Articles;

/// <summary>
/// Barcode payload on the wire — plain strings keyed by barcode format.
/// All members are optional; null means "no barcode in this format".
/// </summary>
public sealed record ArticleBarcodesModel(
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
