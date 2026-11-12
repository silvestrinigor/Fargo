using Fargo.Application.Abstractions;
using Fargo.Application.Operations.Articles.Dtos;
using Fargo.Core.Barcodes;

namespace Fargo.Application.Operations.Articles;

/// <summary>
/// Query used to retrieve an article by barcode.
/// </summary>
/// <param name="ArticleBarcode">
/// Article barcode information.
/// </param>
public sealed record ArticleByBarcodeQuery(Barcode ArticleBarcode) : IQuery<ArticleDto?>;

