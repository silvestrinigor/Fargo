using Fargo.Core.Barcodes;
namespace Fargo.Core.Articles;

/// <summary>
/// Provides domain operations for article barcode assignment and validation.
/// </summary>
public sealed class ArticleService(IArticleRepository articleRepository)
{
    public Task SetEan13(Ean13? value, Article article, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Ean13, (a, barcode) => a.SetEan13(barcode), articleRepository.ExistsByBarcode, BarcodeFormat.Ean13, cancellationToken);

    public Task SetEan8(Ean8? value, Article article, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Ean8, (a, barcode) => a.SetEan8(barcode), articleRepository.ExistsByBarcode, BarcodeFormat.Ean8, cancellationToken);

    public Task SetUpcA(UpcA? value, Article article, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.UpcA, (a, barcode) => a.SetUpcA(barcode), articleRepository.ExistsByBarcode, BarcodeFormat.UpcA, cancellationToken);

    public Task SetUpcE(UpcE? value, Article article, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.UpcE, (a, barcode) => a.SetUpcE(barcode), articleRepository.ExistsByBarcode, BarcodeFormat.UpcE, cancellationToken);

    public Task SetCode128(Code128? value, Article article, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Code128, (a, barcode) => a.SetCode128(barcode), articleRepository.ExistsByBarcode, BarcodeFormat.Code128, cancellationToken);

    public Task SetCode39(Code39? value, Article article, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Code39, (a, barcode) => a.SetCode39(barcode), articleRepository.ExistsByBarcode, BarcodeFormat.Code39, cancellationToken);

    public Task SetItf14(Itf14? value, Article article, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Itf14, (a, barcode) => a.SetItf14(barcode), articleRepository.ExistsByBarcode, BarcodeFormat.Itf14, cancellationToken);

    public Task SetGs1128(Gs1128? value, Article article, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Gs1128, (a, barcode) => a.SetGs1128(barcode), articleRepository.ExistsByBarcode, BarcodeFormat.Gs1128, cancellationToken);

    public Task SetQrCode(QrCode? value, Article article, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.QrCode, (a, barcode) => a.SetQrCode(barcode), articleRepository.ExistsByBarcode, BarcodeFormat.QrCode, cancellationToken);

    public Task SetDataMatrix(DataMatrix? value, Article article, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.DataMatrix, (a, barcode) => a.SetDataMatrix(barcode), articleRepository.ExistsByBarcode, BarcodeFormat.DataMatrix, cancellationToken);

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
