using Fargo.Domain.Barcodes;

namespace Fargo.Application.Exceptions;

/// <summary>
/// Exception thrown when an article already has a barcode with the specified format.
/// </summary>
public class BarcodeAlreadyExistsFargoApplicationException(Guid articleGuid, BarcodeFormat format)
    : FargoApplicationException($"Article '{articleGuid}' already has a barcode with format '{format}'.")
{
    /// <summary>Gets the identifier of the article.</summary>
    public Guid ArticleGuid { get; } = articleGuid;

    /// <summary>Gets the duplicate format.</summary>
    public BarcodeFormat Format { get; } = format;
}
