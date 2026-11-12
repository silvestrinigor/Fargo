using Fargo.Application.Abstractions;
using Fargo.Application.Operations.Articles.Dtos;
using Fargo.Core.Barcodes;

namespace Fargo.Application.Operations.Articles;

public sealed record ArticleByBarcodeQuery(Barcode ArticleBarcode) : IQuery<ArticleDto?>;

