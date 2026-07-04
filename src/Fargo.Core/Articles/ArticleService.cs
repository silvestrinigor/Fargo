using Fargo.Core.Shared.Barcodes;

namespace Fargo.Core.Articles;

public sealed class ArticleService(IArticleRepository articleRepository)
{
    public async Task AssertArticleCanBeDeletedAsync(Article article, CancellationToken cancellationToken = default)
    {
        var hasItems = await articleRepository.HasItemsAssociatedAsync(
            article.Guid,
            cancellationToken);

        if (hasItems)
        {
            throw new ArticleDeleteWithItemsAssociatedFargoDomainException(article.Guid);
        }
    }

    public async Task AssertArticleEan13IsAvailableAsync(Ean13 ean13, CancellationToken cancellationToken = default)
    {
        var exists = await articleRepository.ExistsByEan13Async(ean13, cancellationToken);

        if (exists)
        {
            throw new ArticleBarcodeAlreadyInUseFargoDomainException(new Barcode(ean13.ToString(), BarcodeFormat.Ean13));
        }
    }

    public async Task AssertArticleEan8IsAvailableAsync(Ean8 ean8, CancellationToken cancellationToken = default)
    {
        var exists = await articleRepository.ExistsByEan8Async(ean8, cancellationToken);

        if (exists)
        {
            throw new ArticleBarcodeAlreadyInUseFargoDomainException(new Barcode(ean8.ToString(), BarcodeFormat.Ean8));
        }
    }

    public async Task AssertArticleUpcAIsAvailableAsync(UpcA upcA, CancellationToken cancellationToken = default)
    {
        var exists = await articleRepository.ExistsByUpcAAsync(upcA, cancellationToken);

        if (exists)
        {
            throw new ArticleBarcodeAlreadyInUseFargoDomainException(new Barcode(upcA.ToString(), BarcodeFormat.UpcA));
        }
    }

    public async Task AssertArticleUpcEIsAvailableAsync(UpcE upcE, CancellationToken cancellationToken = default)
    {
        var exists = await articleRepository.ExistsByUpcEAsync(upcE, cancellationToken);

        if (exists)
        {
            throw new ArticleBarcodeAlreadyInUseFargoDomainException(new Barcode(upcE.ToString(), BarcodeFormat.UpcE));
        }
    }

    public async Task AssertArticleCode128IsAvailableAsync(Code128 code128, CancellationToken cancellationToken = default)
    {
        var exists = await articleRepository.ExistsByCode128Async(code128, cancellationToken);

        if (exists)
        {
            throw new ArticleBarcodeAlreadyInUseFargoDomainException(new Barcode(code128.ToString(), BarcodeFormat.Code128));
        }
    }

    public async Task AssertArticleCode39IsAvailableAsync(Code39 code39, CancellationToken cancellationToken = default)
    {
        var exists = await articleRepository.ExistsByCode39Async(code39, cancellationToken);

        if (exists)
        {
            throw new ArticleBarcodeAlreadyInUseFargoDomainException(new Barcode(code39.ToString(), BarcodeFormat.Code39));
        }
    }

    public async Task AssertArticleItf14IsAvailableAsync(Itf14 itf14, CancellationToken cancellationToken = default)
    {
        var exists = await articleRepository.ExistsByItf14Async(itf14, cancellationToken);

        if (exists)
        {
            throw new ArticleBarcodeAlreadyInUseFargoDomainException(new Barcode(itf14.ToString(), BarcodeFormat.Itf14));
        }
    }

    public async Task AssertArticleGs1128IsAvailableAsync(Gs1128 gs1128, CancellationToken cancellationToken = default)
    {
        var exists = await articleRepository.ExistsByGs1128Async(gs1128, cancellationToken);

        if (exists)
        {
            throw new ArticleBarcodeAlreadyInUseFargoDomainException(new Barcode(gs1128.ToString(), BarcodeFormat.Gs1128));
        }
    }

    public async Task AssertArticleQrCodeIsAvailableAsync(QrCode qrCode, CancellationToken cancellationToken = default)
    {
        var exists = await articleRepository.ExistsByQrCodeAsync(qrCode, cancellationToken);

        if (exists)
        {
            throw new ArticleBarcodeAlreadyInUseFargoDomainException(new Barcode(qrCode.ToString(), BarcodeFormat.QrCode));
        }
    }

    public async Task AssertArticleDataMatrixIsAvailableAsync(DataMatrix dataMatrix, CancellationToken cancellationToken = default)
    {
        var exists = await articleRepository.ExistsByDataMatrixAsync(dataMatrix, cancellationToken);

        if (exists)
        {
            throw new ArticleBarcodeAlreadyInUseFargoDomainException(new Barcode(dataMatrix.ToString(), BarcodeFormat.DataMatrix));
        }
    }
}
