using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.Articles;

/// <summary>Low-level HTTP transport for article endpoints.</summary>
public interface IArticleHttpClient
{
    /// <summary>Retrieves a single article by its unique identifier.</summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<ArticleResult>> GetAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged, optionally filtered list of articles.</summary>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">Maximum results per page.</param>
    /// <param name="partitionGuid">Filter to articles in this partition.</param>
    /// <param name="search">An optional search term to filter by name.</param>
    /// <param name="noPartition">When <see langword="true"/>, returns only articles without a partition.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<IReadOnlyCollection<ArticleResult>>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        string? search = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new article and returns its assigned identifier.</summary>
    /// <param name="name">The article name.</param>
    /// <param name="description">An optional description.</param>
    /// <param name="firstPartition">An optional initial partition to assign.</param>
    /// <param name="mass">Optional mass specification.</param>
    /// <param name="lengthX">Optional length along the X axis.</param>
    /// <param name="lengthY">Optional length along the Y axis.</param>
    /// <param name="lengthZ">Optional length along the Z axis.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<Guid>> CreateAsync(
        string name,
        string? description = null,
        Guid? firstPartition = null,
        MassDto? mass = null,
        LengthDto? lengthX = null,
        LengthDto? lengthY = null,
        LengthDto? lengthZ = null,
        CancellationToken cancellationToken = default);

    /// <summary>Updates the properties of an existing article.</summary>
    /// <param name="articleGuid">The unique identifier of the article to update.</param>
    /// <param name="name">The new name, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="description">The new description, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="mass">The new mass, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="lengthX">The new X length, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="lengthY">The new Y length, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="lengthZ">The new Z length, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid articleGuid,
        string? name = null,
        string? description = null,
        MassDto? mass = null,
        LengthDto? lengthX = null,
        LengthDto? lengthY = null,
        LengthDto? lengthZ = null,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an article by its unique identifier.</summary>
    /// <param name="articleGuid">The unique identifier of the article to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a partition to an article.</summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="partitionGuid">The unique identifier of the partition to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(
        Guid articleGuid,
        Guid partitionGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a partition from an article.</summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="partitionGuid">The unique identifier of the partition to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(
        Guid articleGuid,
        Guid partitionGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the partitions assigned to an article.</summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Uploads an image for an article, replacing any existing image.</summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="stream">The image data stream.</param>
    /// <param name="contentType">The MIME type of the image.</param>
    /// <param name="fileName">The file name hint sent with the request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UploadImageAsync(
        Guid articleGuid,
        Stream stream,
        string contentType,
        string fileName = "image",
        CancellationToken cancellationToken = default);

    /// <summary>Deletes the image associated with an article.</summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteImageAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the image stream and content type for an article, or <see langword="null"/> if none exists.</summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<(Stream Stream, string ContentType)?> GetImageAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all barcodes associated with an article.</summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<IReadOnlyCollection<BarcodeResult>>> GetBarcodesAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a barcode to an article and returns its assigned identifier.</summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="code">The barcode value string.</param>
    /// <param name="format">The barcode symbology format.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<Guid>> AddBarcodeAsync(
        Guid articleGuid,
        string code,
        BarcodeFormat format,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a barcode from an article.</summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="barcodeGuid">The unique identifier of the barcode to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> RemoveBarcodeAsync(
        Guid articleGuid,
        Guid barcodeGuid,
        CancellationToken cancellationToken = default);
}
