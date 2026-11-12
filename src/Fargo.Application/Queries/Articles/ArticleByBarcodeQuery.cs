using Fargo.Application.Abstractions;
using Fargo.Application.Dtos.Articles;
using Fargo.Core.Barcodes;

namespace Fargo.Application.Queries.Articles;

public sealed record ArticleByBarcodeQuery(Barcode ArticleBarcode) : IQuery<ArticleDto?>;

