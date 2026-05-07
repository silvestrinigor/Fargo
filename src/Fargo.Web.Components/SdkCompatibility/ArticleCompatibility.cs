using Fargo.Sdk.Contracts.Articles;

namespace Fargo.Sdk.Articles;

public interface IArticleManager
{
    Task<Article> GetAsync(Guid articleGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Article>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default);

    Task<Article> CreateAsync(
        string name,
        string? description = null,
        IReadOnlyCollection<Guid>? partitions = null,
        ArticleBarcodes? barcodes = null,
        ArticleMetrics? metrics = null,
        TimeSpan? shelfLife = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid articleGuid, CancellationToken cancellationToken = default);
}

public sealed class Article
{
    private readonly IArticleHttpClient client;

    internal Article(ArticleInfo result, IArticleHttpClient client)
    {
        this.client = client;
        Guid = result.Guid;
        Name = result.Name;
        Description = result.Description;
        Metrics = result.Metrics.ToSdk();
        ShelfLife = result.ShelfLife;
        Barcodes = result.Barcodes.ToSdk();
        Partitions = result.Partitions.ToArray();
        IsActive = result.IsActive;
        EditedByGuid = result.EditedByGuid;
    }

    public Guid Guid { get; }

    public string Name { get; set; }

    public string Description { get; set; }

    public ArticleMetrics? Metrics { get; set; }

    public TimeSpan? ShelfLife { get; set; }

    public ArticleBarcodes Barcodes { get; set; }

    public IReadOnlyCollection<Guid> Partitions { get; set; }

    public bool IsActive { get; set; }

    public Guid? EditedByGuid { get; }

    public async Task UpdateAsync(Action<Article> update, CancellationToken cancellationToken = default)
    {
        update(this);
        var response = await client.UpdateAsync(
            Guid,
            new ArticleUpdateRequest(
                Name,
                Description,
                Metrics.ToContract(),
                ShelfLife,
                Partitions,
                Barcodes.ToContract(),
                IsActive),
            cancellationToken);
        response.EnsureSuccess("Failed to update article.");
    }
}

internal static class ArticleCompatibilityMappings
{
    public static ArticleMetrics? ToSdk(this ArticleMetricsInfo? contract)
        => contract is null
            ? null
            : new ArticleMetrics
            {
                Mass = contract.Mass is null ? null : new Mass(contract.Mass.Value, Mass.ParseUnit(contract.Mass.Unit)),
                LengthX = contract.LengthX is null ? null : new Length(contract.LengthX.Value, Length.ParseUnit(contract.LengthX.Unit)),
                LengthY = contract.LengthY is null ? null : new Length(contract.LengthY.Value, Length.ParseUnit(contract.LengthY.Unit)),
                LengthZ = contract.LengthZ is null ? null : new Length(contract.LengthZ.Value, Length.ParseUnit(contract.LengthZ.Unit)),
            };

    public static ArticleBarcodes ToSdk(this ArticleBarcodesInfo? contract)
        => contract is null
            ? new ArticleBarcodes()
            : new ArticleBarcodes
            {
                Ean13 = contract.Ean13 is null ? null : new Ean13(Guid.Empty, Guid.Empty, contract.Ean13),
                Ean8 = contract.Ean8 is null ? null : new Ean8(Guid.Empty, Guid.Empty, contract.Ean8),
                UpcA = contract.UpcA is null ? null : new UpcA(Guid.Empty, Guid.Empty, contract.UpcA),
                UpcE = contract.UpcE is null ? null : new UpcE(Guid.Empty, Guid.Empty, contract.UpcE),
                Code128 = contract.Code128 is null ? null : new Code128(Guid.Empty, Guid.Empty, contract.Code128),
                Code39 = contract.Code39 is null ? null : new Code39(Guid.Empty, Guid.Empty, contract.Code39),
                Itf14 = contract.Itf14 is null ? null : new Itf14(Guid.Empty, Guid.Empty, contract.Itf14),
                Gs1128 = contract.Gs1128 is null ? null : new Gs1128(Guid.Empty, Guid.Empty, contract.Gs1128),
                QrCode = contract.QrCode is null ? null : new QrCode(Guid.Empty, Guid.Empty, contract.QrCode),
                DataMatrix = contract.DataMatrix is null ? null : new DataMatrix(Guid.Empty, Guid.Empty, contract.DataMatrix),
            };

    public static ArticleMetricsInfo? ToContract(this ArticleMetrics? metrics)
        => metrics is null
            ? null
            : new ArticleMetricsInfo(
                metrics.Mass is null ? null : new MassInfo(metrics.Mass.Value, metrics.Mass.ToAbbreviation()),
                metrics.LengthX is null ? null : new LengthInfo(metrics.LengthX.Value, metrics.LengthX.ToAbbreviation()),
                metrics.LengthY is null ? null : new LengthInfo(metrics.LengthY.Value, metrics.LengthY.ToAbbreviation()),
                metrics.LengthZ is null ? null : new LengthInfo(metrics.LengthZ.Value, metrics.LengthZ.ToAbbreviation()));

    public static ArticleBarcodesInfo ToContract(this ArticleBarcodes barcodes)
        => new(
            barcodes.Ean13 is null ? null : barcodes.Ean13.Value.Code,
            barcodes.Ean8 is null ? null : barcodes.Ean8.Value.Code,
            barcodes.UpcA is null ? null : barcodes.UpcA.Value.Code,
            barcodes.UpcE is null ? null : barcodes.UpcE.Value.Code,
            barcodes.Code128 is null ? null : barcodes.Code128.Value.Code,
            barcodes.Code39 is null ? null : barcodes.Code39.Value.Code,
            barcodes.Itf14 is null ? null : barcodes.Itf14.Value.Code,
            barcodes.Gs1128 is null ? null : barcodes.Gs1128.Value.Code,
            barcodes.QrCode is null ? null : barcodes.QrCode.Value.Code,
            barcodes.DataMatrix is null ? null : barcodes.DataMatrix.Value.Code);
}

public sealed class ArticleManager(IArticleHttpClient client) : IArticleManager
{
    public async Task<Article> GetAsync(Guid articleGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
        => new((await client.GetAsync(articleGuid, temporalAsOf, cancellationToken)).Unwrap("Failed to load article."), client);

    public async Task<IReadOnlyCollection<Article>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default)
        => (await client.GetManyAsync(temporalAsOf, page, limit, insideAnyOfThisPartitions, notInsideAnyPartition, cancellationToken))
            .Unwrap("Failed to load articles.")
            .Select(x => new Article(x, client))
            .ToArray();

    public async Task<Article> CreateAsync(
        string name,
        string? description = null,
        IReadOnlyCollection<Guid>? partitions = null,
        ArticleBarcodes? barcodes = null,
        ArticleMetrics? metrics = null,
        TimeSpan? shelfLife = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var guid = (await client.CreateAsync(
                new ArticleCreateRequest(
                    name,
                    description,
                    metrics.ToContract(),
                    shelfLife,
                    partitions,
                    barcodes?.ToContract(),
                    isActive),
                cancellationToken))
            .Unwrap("Failed to create article.");

        return await GetAsync(guid, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid articleGuid, CancellationToken cancellationToken = default)
        => (await client.DeleteAsync(articleGuid, cancellationToken)).EnsureSuccess("Failed to delete article.");
}
