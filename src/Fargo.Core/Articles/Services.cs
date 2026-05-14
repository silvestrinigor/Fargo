using Fargo.Core.Barcodes;
using Fargo.Core.Identity;

namespace Fargo.Core.Articles;

#region Services

/// <summary>
/// Provides domain operations for article barcode assignment and validation.
/// </summary>
public sealed class ArticleService(IArticleRepository articleRepository)
{
    public Task SetEan13(Ean13? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, actor, value, a => a.Ean13, (a, barcode, currentActor) => a.SetEan13(barcode, currentActor), articleRepository.ExistsByBarcode, BarcodeFormat.Ean13, cancellationToken);

    public Task SetEan8(Ean8? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, actor, value, a => a.Ean8, (a, barcode, currentActor) => a.SetEan8(barcode, currentActor), articleRepository.ExistsByBarcode, BarcodeFormat.Ean8, cancellationToken);

    public Task SetUpcA(UpcA? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, actor, value, a => a.UpcA, (a, barcode, currentActor) => a.SetUpcA(barcode, currentActor), articleRepository.ExistsByBarcode, BarcodeFormat.UpcA, cancellationToken);

    public Task SetUpcE(UpcE? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, actor, value, a => a.UpcE, (a, barcode, currentActor) => a.SetUpcE(barcode, currentActor), articleRepository.ExistsByBarcode, BarcodeFormat.UpcE, cancellationToken);

    public Task SetCode128(Code128? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, actor, value, a => a.Code128, (a, barcode, currentActor) => a.SetCode128(barcode, currentActor), articleRepository.ExistsByBarcode, BarcodeFormat.Code128, cancellationToken);

    public Task SetCode39(Code39? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, actor, value, a => a.Code39, (a, barcode, currentActor) => a.SetCode39(barcode, currentActor), articleRepository.ExistsByBarcode, BarcodeFormat.Code39, cancellationToken);

    public Task SetItf14(Itf14? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, actor, value, a => a.Itf14, (a, barcode, currentActor) => a.SetItf14(barcode, currentActor), articleRepository.ExistsByBarcode, BarcodeFormat.Itf14, cancellationToken);

    public Task SetGs1128(Gs1128? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, actor, value, a => a.Gs1128, (a, barcode, currentActor) => a.SetGs1128(barcode, currentActor), articleRepository.ExistsByBarcode, BarcodeFormat.Gs1128, cancellationToken);

    public Task SetQrCode(QrCode? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, actor, value, a => a.QrCode, (a, barcode, currentActor) => a.SetQrCode(barcode, currentActor), articleRepository.ExistsByBarcode, BarcodeFormat.QrCode, cancellationToken);

    public Task SetDataMatrix(DataMatrix? value, Article article, Actor actor, CancellationToken cancellationToken = default)
        => SetBarcode(article, actor, value, a => a.DataMatrix, (a, barcode, currentActor) => a.SetDataMatrix(barcode, currentActor), articleRepository.ExistsByBarcode, BarcodeFormat.DataMatrix, cancellationToken);

    private static async Task SetBarcode<TBarcode>(
        Article article,
        Actor actor,
        TBarcode? value,
        Func<Article, TBarcode?> getter,
        Action<Article, TBarcode?, Actor> setter,
        Func<TBarcode, CancellationToken, Task<bool>> existsByBarcode,
        BarcodeFormat format,
        CancellationToken cancellationToken)
        where TBarcode : struct, IEquatable<TBarcode>
    {
        ArgumentNullException.ThrowIfNull(article);
        ArgumentNullException.ThrowIfNull(actor);

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

#endregion Services
