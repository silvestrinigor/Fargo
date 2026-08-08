using Fargo.Application.Common;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Shared.Barcodes;

namespace Fargo.Application.Articles;

/// <summary>
/// Query used to retrieve an article by barcode.
/// </summary>
/// <param name="ArticleBarcode">
/// Article barcode information.
/// </param>
public sealed record ArticleByBarcodeQuery(Barcode ArticleBarcode) : IQuery<ArticleDto?>;
