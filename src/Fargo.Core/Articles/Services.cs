using Fargo.Core.Actors;
using Fargo.Core.Shared.Barcodes;
namespace Fargo.Core.Articles;

/// <summary>
/// Provides domain operations for article barcode assignment and validation.
/// </summary>
public sealed class ArticleService(IArticleRepository articleRepository)
{
    public Task SetEan13(Ean13? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Ean13, (a, barcode, actor) => a.SetEan13(barcode, actor), articleRepository.ExistsByBarcode, BarcodeFormat.Ean13, actor, cancellationToken);

    public Task SetEan8(Ean8? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Ean8, (a, barcode, actor) => a.SetEan8(barcode, actor), articleRepository.ExistsByBarcode, BarcodeFormat.Ean8, actor, cancellationToken);

    public Task SetUpcA(UpcA? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.UpcA, (a, barcode, actor) => a.SetUpcA(barcode, actor), articleRepository.ExistsByBarcode, BarcodeFormat.UpcA, actor, cancellationToken);

    public Task SetUpcE(UpcE? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.UpcE, (a, barcode, actor) => a.SetUpcE(barcode, actor), articleRepository.ExistsByBarcode, BarcodeFormat.UpcE, actor, cancellationToken);

    public Task SetCode128(Code128? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Code128, (a, barcode, actor) => a.SetCode128(barcode, actor), articleRepository.ExistsByBarcode, BarcodeFormat.Code128, actor, cancellationToken);

    public Task SetCode39(Code39? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Code39, (a, barcode, actor) => a.SetCode39(barcode, actor), articleRepository.ExistsByBarcode, BarcodeFormat.Code39, actor, cancellationToken);

    public Task SetItf14(Itf14? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Itf14, (a, barcode, actor) => a.SetItf14(barcode, actor), articleRepository.ExistsByBarcode, BarcodeFormat.Itf14, actor, cancellationToken);

    public Task SetGs1128(Gs1128? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.Gs1128, (a, barcode, actor) => a.SetGs1128(barcode, actor), articleRepository.ExistsByBarcode, BarcodeFormat.Gs1128, actor, cancellationToken);

    public Task SetQrCode(QrCode? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.QrCode, (a, barcode, actor) => a.SetQrCode(barcode, actor), articleRepository.ExistsByBarcode, BarcodeFormat.QrCode, actor, cancellationToken);

    public Task SetDataMatrix(DataMatrix? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, value, a => a.DataMatrix, (a, barcode, actor) => a.SetDataMatrix(barcode, actor), articleRepository.ExistsByBarcode, BarcodeFormat.DataMatrix, actor, cancellationToken);

    private static async Task SetBarcode<TBarcode>(
        Article article,
        TBarcode? value,
        Func<Article, TBarcode?> getter,
        Action<Article, TBarcode?, Actor> setter,
        Func<TBarcode, CancellationToken, Task<bool>> existsByBarcode,
        BarcodeFormat format,
        Actor actor,
        CancellationToken cancellationToken)
        where TBarcode : struct, IEquatable<TBarcode>
    {
        ArgumentNullException.ThrowIfNull(article);
        article.ValidateCanEdit(actor);

        if (EqualityComparer<TBarcode?>.Default.Equals(getter(article), value))
        {
            return;
        }

        if (value is { } barcode && await existsByBarcode(barcode, cancellationToken))
        {
            throw new ArticleBarcodeAlreadyInUseFargoDomainException(format, barcode.ToString()!);
        }

        setter(article, value, actor);
    }
}
