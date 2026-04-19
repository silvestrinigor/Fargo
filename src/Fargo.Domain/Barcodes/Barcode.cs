using Fargo.Domain.Articles;

namespace Fargo.Domain.Barcodes;

/// <summary>
/// Represents a barcode associated with an article.
/// </summary>
/// <remarks>
/// Barcodes are immutable after creation. An article may have at most one barcode
/// per format, enforced at both the collection and database levels.
/// The <see cref="Value"/> property is a computed convenience accessor over the
/// stored <see cref="Code"/> and <see cref="Format"/> columns; it is not mapped
/// to a database column.
/// </remarks>
public class Barcode : Entity
{
    /// <summary>
    /// Gets the raw barcode code string as stored in the database.
    /// Set by EF Core on load, or via the <see cref="Value"/> init accessor on creation.
    /// </summary>
    public string Code { get; private init; } = null!;

    /// <summary>
    /// Gets the barcode format (symbology) as stored in the database.
    /// Set by EF Core on load, or via the <see cref="Value"/> init accessor on creation.
    /// </summary>
    public BarcodeFormat Format { get; private init; }

    /// <summary>
    /// Gets or initializes the barcode value object combining <see cref="Code"/> and <see cref="Format"/>.
    /// This property is not mapped to a column; it is computed from the stored fields on get,
    /// and sets <see cref="Code"/> and <see cref="Format"/> on init.
    /// </summary>
    public required BarcodeValue Value
    {
        get => BarcodeValue.FromStorage(Code, Format);
        init
        {
            Code = value.Code;
            Format = value.Format;
        }
    }

    /// <summary>
    /// Gets the unique identifier of the article this barcode belongs to.
    /// </summary>
    public Guid ArticleGuid { get; private init; }

    /// <summary>
    /// Gets the article this barcode belongs to.
    /// Setting this navigation property also sets <see cref="ArticleGuid"/>.
    /// </summary>
    public required Article Article
    {
        get;
        init
        {
            field = value;
            ArticleGuid = value.Guid;
        }
    }
}
