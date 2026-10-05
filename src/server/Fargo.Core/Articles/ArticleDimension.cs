using UnitsNet;

namespace Fargo.Core.Articles;

/// <summary>
/// Represents the physical dimensions of an <see cref="Article"/>.
/// </summary>
/// <remarks>
/// Dimensions are optional. When one or more dimensions are
/// <see langword="null"/>, the article does not have a complete set of
/// physical measurements.
/// </remarks>
public class ArticleDimension
{
    /// <summary>
    /// Gets the unique identifier of the associated article.
    /// </summary>
    public Guid ArticleGuid { get; private init; }

    /// <summary>
    /// Gets the article associated with these dimensions.
    /// </summary>
    public Article Article { get; private init; }

    /// <summary>
    /// Gets the length of the article along the X axis.
    /// </summary>
    public Length? X { get; private set; }

    /// <summary>
    /// Gets the length of the article along the Y axis.
    /// </summary>
    public Length? Y { get; private set; }

    /// <summary>
    /// Gets the length of the article along the Z axis.
    /// </summary>
    public Length? Z { get; private set; }

    /// <summary>
    /// Initializes a new <see cref="ArticleDimension"/> instance.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ArticleDimension() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Initializes the dimensions associated with the specified article.
    /// </summary>
    /// <param name="article">
    /// The associated article.
    /// </param>
    internal ArticleDimension(Article article)
    {
        Article = article;
        ArticleGuid = article.Guid;
    }

    /// <summary>
    /// Sets the physical dimensions of the article.
    /// </summary>
    /// <param name="lengthX">
    /// The length along the X axis.
    /// </param>
    /// <param name="lengthY">
    /// The length along the Y axis.
    /// </param>
    /// <param name="lengthZ">
    /// The length along the Z axis.
    /// </param>
    /// <remarks>
    /// Passing <see langword="null"/> for a dimension removes the corresponding
    /// measurement.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any specified dimension is less than or equal to zero.
    /// </exception>
    internal void SetDimensions(Length? lengthX, Length? lengthY, Length? lengthZ)
    {
        ValidateDimension(lengthX, nameof(lengthX));
        ValidateDimension(lengthY, nameof(lengthY));
        ValidateDimension(lengthZ, nameof(lengthZ));

        X = lengthX;
        Y = lengthY;
        Z = lengthZ;
    }

    private static void ValidateDimension(Length? length, string paramName)
    {
        if (length is not null && length.Value <= Length.Zero)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                length,
                "Dimensions must be greater than zero.");
        }
    }
}
