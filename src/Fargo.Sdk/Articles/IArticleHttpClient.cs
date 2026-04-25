using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.Articles;

/// <summary>Low-level HTTP transport for article endpoints.</summary>
public interface IArticleHttpClient
{
    Task<FargoSdkResponse<ArticleResult>> GetAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<IReadOnlyCollection<ArticleResult>>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        string? search = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<Guid>> CreateAsync(
        string name,
        string? description = null,
        Guid? firstPartition = null,
        MassDto? mass = null,
        LengthDto? lengthX = null,
        LengthDto? lengthY = null,
        LengthDto? lengthZ = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid articleGuid,
        string? name = null,
        string? description = null,
        MassDto? mass = null,
        LengthDto? lengthX = null,
        LengthDto? lengthY = null,
        LengthDto? lengthZ = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(
        Guid articleGuid,
        Guid partitionGuid,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(
        Guid articleGuid,
        Guid partitionGuid,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> UploadImageAsync(
        Guid articleGuid,
        Stream stream,
        string contentType,
        string fileName = "image",
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> DeleteImageAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    Task<(Stream Stream, string ContentType)?> GetImageAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<IReadOnlyCollection<BarcodeResult>>> GetBarcodesAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<Guid>> AddBarcodeAsync(
        Guid articleGuid,
        string code,
        BarcodeFormat format,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> RemoveBarcodeAsync(
        Guid articleGuid,
        Guid barcodeGuid,
        CancellationToken cancellationToken = default);
}
