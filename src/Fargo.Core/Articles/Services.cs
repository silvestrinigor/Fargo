using Fargo.Core.Barcodes;

namespace Fargo.Core.Articles;

#region Services

/// <summary>
/// Provides domain operations for article barcode assignment and validation.
/// </summary>
public sealed class ArticleService(IArticleRepository articleRepository)
{
    public Task SetEan13(Article article, Ean13? value, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Ean13, (a, barcode) => a.Ean13 = barcode, articleRepository.ExistsByBarcode, BarcodeFormat.Ean13, cancellationToken);

    public Task SetEan8(Article article, Ean8? value, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Ean8, (a, barcode) => a.Ean8 = barcode, articleRepository.ExistsByBarcode, BarcodeFormat.Ean8, cancellationToken);

    public Task SetUpcA(Article article, UpcA? value, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.UpcA, (a, barcode) => a.UpcA = barcode, articleRepository.ExistsByBarcode, BarcodeFormat.UpcA, cancellationToken);

    public Task SetUpcE(Article article, UpcE? value, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.UpcE, (a, barcode) => a.UpcE = barcode, articleRepository.ExistsByBarcode, BarcodeFormat.UpcE, cancellationToken);

    public Task SetCode128(Article article, Code128? value, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Code128, (a, barcode) => a.Code128 = barcode, articleRepository.ExistsByBarcode, BarcodeFormat.Code128, cancellationToken);

    public Task SetCode39(Article article, Code39? value, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Code39, (a, barcode) => a.Code39 = barcode, articleRepository.ExistsByBarcode, BarcodeFormat.Code39, cancellationToken);

    public Task SetItf14(Article article, Itf14? value, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Itf14, (a, barcode) => a.Itf14 = barcode, articleRepository.ExistsByBarcode, BarcodeFormat.Itf14, cancellationToken);

    public Task SetGs1128(Article article, Gs1128? value, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Gs1128, (a, barcode) => a.Gs1128 = barcode, articleRepository.ExistsByBarcode, BarcodeFormat.Gs1128, cancellationToken);

    public Task SetQrCode(Article article, QrCode? value, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.QrCode, (a, barcode) => a.QrCode = barcode, articleRepository.ExistsByBarcode, BarcodeFormat.QrCode, cancellationToken);

    public Task SetDataMatrix(Article article, DataMatrix? value, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.DataMatrix, (a, barcode) => a.DataMatrix = barcode, articleRepository.ExistsByBarcode, BarcodeFormat.DataMatrix, cancellationToken);

    private static async Task SetBarcode<TBarcode>(
        Article article,
        TBarcode? value,
        Func<Article, TBarcode?> getter,
        Action<Article, TBarcode?> setter,
        Func<TBarcode, CancellationToken, Task<bool>> existsByBarcode,
        BarcodeFormat format,
        CancellationToken cancellationToken)
        where TBarcode : struct, IEquatable<TBarcode>
    {
        ArgumentNullException.ThrowIfNull(article);

        if (EqualityComparer<TBarcode?>.Default.Equals(getter(article), value))
        {
            return;
        }

        if (value is { } barcode && await existsByBarcode(barcode, cancellationToken))
        {
            throw new ArticleBarcodeAlreadyInUseFargoDomainException(format, barcode.ToString()!);
        }

        setter(article, value);
    }
}

#endregion Services
