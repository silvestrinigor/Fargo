using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
using NSubstitute;

namespace Fargo.Core.Tests.Articles;

public sealed class ArticleServiceTests
{
    private readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();

    [Theory]
    [MemberData(nameof(BarcodeKinds))]
    public async Task SetBarcode_Should_SetValue(string barcodeKind)
    {
        var article = CreateArticle();
        var sut = new ArticleService(articleRepository);
        var value = CreateBarcode(barcodeKind, variant: 0);

        await InvokeSet(sut, article, barcodeKind, value);

        Assert.Equal(value, ReadBarcode(article, barcodeKind));
    }

    [Theory]
    [MemberData(nameof(BarcodeKinds))]
    public async Task SetBarcode_Should_ClearValue(string barcodeKind)
    {
        var article = CreateArticle();
        var sut = new ArticleService(articleRepository);
        var current = CreateBarcode(barcodeKind, variant: 0);

        await WriteBarcode(sut, article, barcodeKind, current);

        await InvokeSet(sut, article, barcodeKind, null);

        Assert.Null(ReadBarcode(article, barcodeKind));
    }

    [Theory]
    [MemberData(nameof(BarcodeKinds))]
    public async Task SetBarcode_Should_NoOp_When_ValueIsUnchanged(string barcodeKind)
    {
        var article = CreateArticle();
        var sut = new ArticleService(articleRepository);
        var current = CreateBarcode(barcodeKind, variant: 0);

        await WriteBarcode(sut, article, barcodeKind, current);
        ConfigureExists(articleRepository, barcodeKind, current, returns: true);

        await InvokeSet(sut, article, barcodeKind, current);

        Assert.Equal(current, ReadBarcode(article, barcodeKind));
    }

    [Theory]
    [MemberData(nameof(BarcodeKinds))]
    public async Task SetBarcode_Should_Throw_When_ValueAlreadyExists(string barcodeKind)
    {
        var article = CreateArticle();
        var sut = new ArticleService(articleRepository);
        var current = CreateBarcode(barcodeKind, variant: 0);
        var next = CreateBarcode(barcodeKind, variant: 1);

        await WriteBarcode(sut, article, barcodeKind, current);
        ConfigureExists(articleRepository, barcodeKind, next, returns: true);

        var exception = await Assert.ThrowsAsync<ArticleBarcodeAlreadyInUseFargoDomainException>(
            () => InvokeSet(sut, article, barcodeKind, next));

        Assert.Equal(next.ToString(), exception.Code);
    }

    public static IEnumerable<object[]> BarcodeKinds()
    {
        yield return new object[] { "Ean13" };
        yield return new object[] { "Ean8" };
        yield return new object[] { "UpcA" };
        yield return new object[] { "UpcE" };
        yield return new object[] { "Code128" };
        yield return new object[] { "Code39" };
        yield return new object[] { "Itf14" };
        yield return new object[] { "Gs1128" };
        yield return new object[] { "QrCode" };
        yield return new object[] { "DataMatrix" };
    }

    private static Article CreateArticle()
    {
        var article = Article.CreateArticle(new Name("Test article"));

        article.ChangeDescription(new Description("Test description"));

        return article;
    }

    private static async Task InvokeSet(
        ArticleService service,
        Article article,
        string barcodeKind,
        object? value)
    {
        switch (barcodeKind)
        {
            case "Ean13":
                await service.SetEan13((Ean13?)value, article);
                return;
            case "Ean8":
                await service.SetEan8((Ean8?)value, article);
                return;
            case "UpcA":
                await service.SetUpcA((UpcA?)value, article);
                return;
            case "UpcE":
                await service.SetUpcE((UpcE?)value, article);
                return;
            case "Code128":
                await service.SetCode128((Code128?)value, article);
                return;
            case "Code39":
                await service.SetCode39((Code39?)value, article);
                return;
            case "Itf14":
                await service.SetItf14((Itf14?)value, article);
                return;
            case "Gs1128":
                await service.SetGs1128((Gs1128?)value, article);
                return;
            case "QrCode":
                await service.SetQrCode((QrCode?)value, article);
                return;
            case "DataMatrix":
                await service.SetDataMatrix((DataMatrix?)value, article);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(barcodeKind), barcodeKind, "Unsupported barcode kind.");
        }
    }

    private static object? ReadBarcode(Article article, string barcodeKind)
        => barcodeKind switch
        {
            "Ean13" => article.Ean13,
            "Ean8" => article.Ean8,
            "UpcA" => article.UpcA,
            "UpcE" => article.UpcE,
            "Code128" => article.Code128,
            "Code39" => article.Code39,
            "Itf14" => article.Itf14,
            "Gs1128" => article.Gs1128,
            "QrCode" => article.QrCode,
            "DataMatrix" => article.DataMatrix,
            _ => throw new ArgumentOutOfRangeException(nameof(barcodeKind), barcodeKind, "Unsupported barcode kind.")
        };

    private static async Task WriteBarcode(ArticleService service, Article article, string barcodeKind, object? value)
    {
        switch (barcodeKind)
        {
            case "Ean13":
                await service.SetEan13((Ean13?)value, article);
                return;
            case "Ean8":
                await service.SetEan8((Ean8?)value, article);
                return;
            case "UpcA":
                await service.SetUpcA((UpcA?)value, article);
                return;
            case "UpcE":
                await service.SetUpcE((UpcE?)value, article);
                return;
            case "Code128":
                await service.SetCode128((Code128?)value, article);
                return;
            case "Code39":
                await service.SetCode39((Code39?)value, article);
                return;
            case "Itf14":
                await service.SetItf14((Itf14?)value, article);
                return;
            case "Gs1128":
                await service.SetGs1128((Gs1128?)value, article);
                return;
            case "QrCode":
                await service.SetQrCode((QrCode?)value, article);
                return;
            case "DataMatrix":
                await service.SetDataMatrix((DataMatrix?)value, article);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(barcodeKind), barcodeKind, "Unsupported barcode kind.");
        }
    }

    private static void ConfigureExists(
        IArticleRepository repository,
        string barcodeKind,
        object? barcode,
        bool returns)
    {
        switch (barcodeKind)
        {
            case "Ean13":
                repository.ExistsByBarcode((Ean13)barcode!).Returns(returns);
                return;
            case "Ean8":
                repository.ExistsByBarcode((Ean8)barcode!).Returns(returns);
                return;
            case "UpcA":
                repository.ExistsByBarcode((UpcA)barcode!).Returns(returns);
                return;
            case "UpcE":
                repository.ExistsByBarcode((UpcE)barcode!).Returns(returns);
                return;
            case "Code128":
                repository.ExistsByBarcode((Code128)barcode!).Returns(returns);
                return;
            case "Code39":
                repository.ExistsByBarcode((Code39)barcode!).Returns(returns);
                return;
            case "Itf14":
                repository.ExistsByBarcode((Itf14)barcode!).Returns(returns);
                return;
            case "Gs1128":
                repository.ExistsByBarcode((Gs1128)barcode!).Returns(returns);
                return;
            case "QrCode":
                repository.ExistsByBarcode((QrCode)barcode!).Returns(returns);
                return;
            case "DataMatrix":
                repository.ExistsByBarcode((DataMatrix)barcode!).Returns(returns);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(barcodeKind), barcodeKind, "Unsupported barcode kind.");
        }
    }

    private static object CreateBarcode(string barcodeKind, int variant)
        => barcodeKind switch
        {
            "Ean13" => new Ean13(variant == 0 ? "1234567890123" : "9876543210987"),
            "Ean8" => new Ean8(variant == 0 ? "12345670" : "76543210"),
            "UpcA" => new UpcA(variant == 0 ? "123456789012" : "987654321098"),
            "UpcE" => new UpcE(variant == 0 ? "12345670" : "76543210"),
            "Code128" => new Code128(variant == 0 ? "ABC-123" : "XYZ-789"),
            "Code39" => new Code39(variant == 0 ? "CODE39A" : "CODE39B"),
            "Itf14" => new Itf14(variant == 0 ? "12345678901234" : "98765432109876"),
            "Gs1128" => new Gs1128(variant == 0 ? "GS1-128-A" : "GS1-128-B"),
            "QrCode" => new QrCode(variant == 0 ? "QR-123" : "QR-456"),
            "DataMatrix" => new DataMatrix(variant == 0 ? "DM-123" : "DM-456"),
            _ => throw new ArgumentOutOfRangeException(nameof(barcodeKind), barcodeKind, "Unsupported barcode kind.")
        };

}
